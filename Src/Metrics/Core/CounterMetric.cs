using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        private readonly ConcurrentDictionary<string, LongAdder> setCounters = new ConcurrentDictionary<string, LongAdder>();

        private readonly LongAdder counter = new LongAdder();

        public CounterValue Value
        {
            get
            {
                return setCounters.Count == 0 ? new CounterValue(this.counter.Value, noItems) : GetValueWithSetItems();
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
            this.counter.SetValue(0L);
            foreach (var item in this.setCounters)
            {
                item.Value.SetValue(0L);
            }
        }

        public void Increment(string item)
        {
            this.Increment();
            SetCounter(item).Increment();
        }

        public void Increment(string item, long amount)
        {
            this.Increment(amount);
            SetCounter(item).Add(amount);
        }

        public void Decrement(string item)
        {
            this.Decrement();
            SetCounter(item).Decrement();
        }

        public void Decrement(string item, long amount)
        {
            this.Decrement(amount);
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
            foreach (var setCounter in cOther.setCounters)
            {
                SetCounter(setCounter.Key).Add(setCounter.Value.Value);
            }

            return true;
        }

        private LongAdder SetCounter(string item)
        {
            return this.setCounters.GetOrAdd(item, v => new LongAdder());
        }

        private CounterValue GetValueWithSetItems()
        {
            var total = this.counter.Value;

            var items = new CounterValue.SetItem[setCounters.Count];
            var index = 0;
            foreach (var item in setCounters)
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
    }
}
