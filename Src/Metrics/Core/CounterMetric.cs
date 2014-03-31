using Metrics.Utils;

namespace Metrics.Core
{
    public class CounterMetric : Counter
    {
        private readonly AtomicLong value = new AtomicLong();

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
    }
}
