using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Metrics.MetricData;
using Metrics.Utils;

namespace Metrics.Core
{
    public interface CounterImplementation : Counter, MetricValueProvider<CounterValue> { }

    public sealed class CounterMetric : CounterImplementation
    {
        private static readonly CounterValue.SetItem[] noItems = new CounterValue.SetItem[0];

        private static readonly IComparer<CounterValue.SetItem> setItemComparer = Comparer<CounterValue.SetItem>.Create((x, y) => x.Percent != y.Percent ?
                Comparer<double>.Default.Compare(x.Percent, y.Percent) :
                Comparer<string>.Default.Compare(x.Item, y.Item));

        private ConcurrentDictionary<string, ThreadLocalLongAdder> setCounters = null;

        private readonly ThreadLocalLongAdder counter = new ThreadLocalLongAdder();

        public CounterValue Value
        {
            get
            {
                if (this.setCounters == null || this.setCounters.Count == 0)
                {
                    return new CounterValue(this.counter.Value, noItems);
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

            Increment(cOther.counter.Value);

            if (cOther.setCounters != null)
            {
                foreach (var setCounter in cOther.setCounters)
                {
                    SetCounter(setCounter.Key).Add(setCounter.Value.Value);
                }
            }

            return true;
        }

        private ThreadLocalLongAdder SetCounter(string item)
        {
            return this.setCounters.GetOrAdd(item, v => new ThreadLocalLongAdder());
        }

        private CounterValue GetValueWithSetItems()
        {
            Debug.Assert(this.setCounters != null);
            var total = this.counter.Value;

            var items = new CounterValue.SetItem[this.setCounters.Count];
            var index = 0;
            foreach (var item in this.setCounters)
            {
                var itemValue = item.Value.Value;

                var percent = total > 0 ? itemValue / (double)total * 100 : 0.0;
                var setCounter = new CounterValue.SetItem(item.Key, itemValue, percent);
                items[index] = setCounter;
                index++;
                if (index == items.Length)
                {
                    break;
                }
            }

            Array.Sort(items, setItemComparer);

            return new CounterValue(total, items);
        }

        private void EnsureSetCountersCreated()
        {
            if (this.setCounters == null)
            {
                this.setCounters = new ConcurrentDictionary<string, ThreadLocalLongAdder>();
            }
            Debug.Assert(this.setCounters != null);
        }
    }
}
