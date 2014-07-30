using Metrics.Utils;

namespace Metrics.Tests.TestUtils
{
    public sealed class TestCounter : Counter, MetricValueProvider<long>
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
        }

        public void Decrement(long value)
        {
        }
    }
}
