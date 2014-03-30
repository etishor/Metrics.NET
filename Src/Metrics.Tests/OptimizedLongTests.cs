using FluentAssertions;
using Metrics.Utils;
using Xunit;

namespace Metrics.Tests
{
    public class OptimizedLongTests
    {
        [Fact]
        public void OptimizedLongDefaultsToZero()
        {
            new AtomicLong().Value.Should().Be(0L);
        }

        [Fact]
        public void OptimizedLongCanBeCreatedWithValue()
        {
            new AtomicLong(5L).Value.Should().Be(5L);
        }

        [Fact]
        public void OptimizedLongCanBeIncremented()
        {
            AtomicLong l = new AtomicLong();
            l.Increment();
            l.Value.Should().Be(1L);
        }

        [Fact]
        public void OptimizedLongCanBeIncrementedMultipleTimes()
        {
            AtomicLong l = new AtomicLong();
            l.Increment();
            l.Increment();
            l.Increment();
            l.Value.Should().Be(3L);
        }

        [Fact]
        public void OptimizedLongCanAddValue()
        {
            AtomicLong l = new AtomicLong();
            l.Add(7L);
            l.Value.Should().Be(7L);
        }

        [Fact]
        public void OptimizedLongCanBeDecremented()
        {
            AtomicLong l = new AtomicLong(10L);
            l.Decrement();
            l.Value.Should().Be(9L);
        }

        [Fact]
        public void OptimizedLongCanBeAssigned()
        {
            AtomicLong x = new AtomicLong(10L);
            AtomicLong y = x;

            y.Value.Should().Be(10L);
        }

    }
}
