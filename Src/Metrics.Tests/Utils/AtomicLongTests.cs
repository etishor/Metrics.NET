using FluentAssertions;
using Metrics.Utils;
using Xunit;

namespace Metrics.Tests.Utils
{
    public class AtomicLongTests
    {
        private AtomicLong num = new AtomicLong();

        [Fact]
        public void AtomicLong_DefaultsToZero()
        {
            num.Value.Should().Be(0L);
        }

        [Fact]
        public void AtomicLong_CanBeCreatedWithValue()
        {
            new AtomicLong(5L).Value.Should().Be(5L);
        }

        [Fact]
        public void AtomicLong_CanSetAndReadValue()
        {
            num.SetValue(32);
            num.Value.Should().Be(32);
        }

        [Fact]
        public void AtomicLong_CanGetAndSet()
        {
            num.SetValue(32);
            long val = num.GetAndSet(64);
            val.Should().Be(32);
            num.Value.Should().Be(64);
        }

        [Fact]
        public void AtomicLong_CanBeIncremented()
        {
            num.Increment();
            num.Value.Should().Be(1L);
        }

        [Fact]
        public void AtomicLong_CanBeIncrementedMultipleTimes()
        {
            num.Increment();
            num.Increment();
            num.Increment();
            num.Value.Should().Be(3L);
        }

        [Fact]
        public void AtomicLong_CanAddValue()
        {
            num.Add(7L);
            num.Value.Should().Be(7L);
        }

        [Fact]
        public void AtomicLong_CanBeDecremented()
        {
            num = new AtomicLong(10L);
            num.Decrement();
            num.Value.Should().Be(9L);
        }

        [Fact]
        public void AtomicLong_CanBeAssigned()
        {
            AtomicLong x = new AtomicLong(10L);
            AtomicLong y = x;
            y.Value.Should().Be(10L);
        }

    }
}
