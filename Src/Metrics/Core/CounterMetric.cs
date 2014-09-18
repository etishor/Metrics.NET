using Metrics.Utils;

namespace Metrics.Core
{
    public interface CounterImplementation : Counter, MetricValueProvider<long> { }

    public sealed class CounterMetric : CounterImplementation
    {
        private AtomicLong value = new AtomicLong();

        public long Value { get { return this.value.Value; } }

        public void Increment()
        {
            this.value.Increment();
        }

        public void Increment(long value)
        {
            this.value.Add(value);
        }

        public void Decrement()
        {
            this.value.Decrement();
        }

        public void Decrement(long value)
        {
            this.value.Add(-value);
        }

        public void Reset()
        {
            this.value.SetValue(0L);
        }
    }
}
