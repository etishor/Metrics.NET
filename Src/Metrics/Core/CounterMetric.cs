using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Metrics.MetricData;

namespace Metrics.Core
{
    public interface CounterImplementation : Counter, MetricValueProvider<CounterValue> { }

    public sealed class CounterMetric : CounterImplementation
    {
        private ConcurrentDictionary<string, ThreadLocalLongAdder> setCounters = null;

        private readonly ThreadLocalLongAdder counter = new ThreadLocalLongAdder();

        public CounterValue Value
        {
            get
            {
                if (this.setCounters == null || this.setCounters.Count == 0)
                {
                    return new CounterValue(this.counter.GetValue());
                }
                return GetValueWithSetItems();
            }
        }

        public CounterValue GetValue(bool resetMetric = false)
        {
            var value = this.Value;
            if (resetMetric)
            {
                this.Reset();
            }
            return value;
        }

        public void Increment()
        {
            this.counter.Increment();
        }

        public void Increment(long value)
        {
            this.counter.Add(value);
        }

        public void Decrement()
        {
            this.counter.Decrement();
        }

        public void Decrement(long value)
        {
            this.counter.Add(-value);
        }

        public void Reset()
        {
            this.counter.Reset();
            if (this.setCounters != null)
            {
                foreach (var item in this.setCounters)
                {
                    item.Value.Reset();
                }
            }
        }

        public void Increment(string item)
        {
            Increment();
            SetCounter(item).Increment();
        }

        public void Increment(string item, long amount)
        {
            Increment(amount);
            SetCounter(item).Add(amount);
        }

        public void Decrement(string item)
        {
            Decrement();
            SetCounter(item).Decrement();
        }

        public void Decrement(string item, long amount)
        {
            Decrement(amount);
            SetCounter(item).Add(-amount);
        }

        public bool Merge(MetricValueProvider<CounterValue> other)
        {
            var cOther = other as CounterMetric;
            if (cOther == null)
            {
                return false;
            }

            Increment(cOther.counter.GetValue());

            if (cOther.setCounters != null)
            {
                foreach (var setCounter in cOther.setCounters)
                {
                    SetCounter(setCounter.Key).Add(setCounter.Value.GetValue());
                }
            }

            return true;
        }

        private ThreadLocalLongAdder SetCounter(string item)
        {
            if (this.setCounters == null)
            {
                Interlocked.CompareExchange(ref this.setCounters, new ConcurrentDictionary<string, ThreadLocalLongAdder>(), null);
            }
            Debug.Assert(this.setCounters != null);
            return this.setCounters.GetOrAdd(item, v => new ThreadLocalLongAdder());
        }

        private CounterValue GetValueWithSetItems()
        {
            Debug.Assert(this.setCounters != null);
            var total = this.counter.GetValue();

            var items = new CounterValue.SetItem[this.setCounters.Count];
            var index = 0;
            foreach (var item in this.setCounters)
            {
                var itemValue = item.Value.GetValue();

                var percent = total > 0 ? itemValue / (double)total * 100 : 0.0;
                var setCounter = new CounterValue.SetItem(item.Key, itemValue, percent);
                items[index++] = setCounter;
                if (index == items.Length)
                {
                    break;
                }
            }

            Array.Sort(items, CounterValue.SetItemComparer);

            return new CounterValue(total, items);
        }
    }
}
