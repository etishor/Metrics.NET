using System.Collections.Concurrent;
using System.Linq;
using Metrics.MetricData;
using Metrics.Utils;

namespace Metrics.Core
{
    public interface CounterImplementation : Counter, MetricValueProvider<CounterValue> { }

    public sealed class CounterMetric : CounterImplementation
    {
        private readonly ConcurrentDictionary<string, AtomicLongHolder> setCounters = new ConcurrentDictionary<string, AtomicLongHolder>();

        private AtomicLong counter = new AtomicLong();

        public CounterValue Value
        {
            get
            {
                var total = this.counter.Value;
                var items = setCounters.Select(i => new CounterValue.SetItem(i.Key, i.Value.Value, total > 0 ? i.Value.Value / (double)total * 100 : 0.0))
                    .OrderBy(i => i.Count)
                    .ThenBy(i => i.Item)
                    .ToArray();
                return new CounterValue(total, items);
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

        private AtomicLongHolder SetCounter(string item)
        {
            return this.setCounters.GetOrAdd(item, v => new AtomicLongHolder());
        }
    }
}
