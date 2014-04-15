using FluentAssertions;
using Metrics.Utils;
using Xunit;

namespace Metrics.Tests
{
    public class AtomicLongTests
    {
        [Fact]
        public void AtomicLongDefaultsToZero()
        {
            new AtomicLong().Value.Should().Be(0L);
        }

        [Fact]
        public void AtomicLongCanBeCreatedWithValue()
        {
            new AtomicLong(5L).Value.Should().Be(5L);
        }

        [Fact]
        public void AtomicLongCanSetAndReadValue()
        {
            var num = new AtomicLong();
            num.SetValue(32);
            num.Value.Should().Be(32);
        }

        [Fact]
        public void AtomicLongCanBeIncremented()
        {
            AtomicLong l = new AtomicLong();
            l.Increment();
            l.Value.Should().Be(1L);
        }

        [Fact]
        public void AtomicLongCanBeIncrementedMultipleTimes()
        {
            AtomicLong l = new AtomicLong();
            l.Increment();
            l.Increment();
            l.Increment();
            l.Value.Should().Be(3L);
        }

        [Fact]
        public void AtomicLongCanAddValue()
        {
            AtomicLong l = new AtomicLong();
            l.Add(7L);
            l.Value.Should().Be(7L);
        }

        [Fact]
        public void AtomicLongCanBeDecremented()
        {
            AtomicLong l = new AtomicLong(10L);
            l.Decrement();
            l.Value.Should().Be(9L);
        }

        [Fact]
        public void AtomicLongCanBeAssigned()
        {
            AtomicLong x = new AtomicLong(10L);
            AtomicLong y = x;

            y.Value.Should().Be(10L);
        }

    }
}
