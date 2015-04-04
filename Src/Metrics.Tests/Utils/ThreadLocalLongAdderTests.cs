using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Metrics.Utils;
using Xunit;

namespace Metrics.Tests.Utils
{
    public class ThreadLocalLongAdderTests
    {
        private ThreadLocalLongAdder num = new ThreadLocalLongAdder();

        [Fact]
        public void ThreadLocalLongAdder_DefaultsToZero()
        {
            num.Value.Should().Be(0L);
        }

        [Fact]
        public void ThreadLocalLongAdder_CanBeCreatedWithValue()
        {
            new AtomicLong(5L).Value.Should().Be(5L);
        }

        [Fact]
        public void ThreadLocalLongAdder_CanSetAndReadValue()
        {
            num.SetValue(32);
            num.Value.Should().Be(32);
        }

        [Fact]
        public void ThreadLocalLongAdder_CanGetAndSet()
        {
            num.SetValue(32);
            long val = num.GetAndSet(64);
            val.Should().Be(32);
            num.Value.Should().Be(64);
        }

        [Fact]
        public void ThreadLocalLongAdder_CanBeIncremented()
        {
            num.Increment();
            num.Value.Should().Be(1L);
        }

        [Fact]
        public void ThreadLocalLongAdder_CanBeIncrementedMultipleTimes()
        {
            num.Increment();
            num.Increment();
            num.Increment();
            num.Value.Should().Be(3L);
        }

        [Fact]
        public void ThreadLocalLongAdder_CanAddValue()
        {
            num.Add(7L);
            num.Value.Should().Be(7L);
        }

        [Fact]
        public void ThreadLocalLongAdder_CanBeDecremented()
        {
            num = new ThreadLocalLongAdder(10L);
            num.Decrement();
            num.Value.Should().Be(9L);
        }

        [Fact]
        public void ThreadLocalLongAdder_CanBeAssigned()
        {
            JavaLongAdder x = new JavaLongAdder(10L);
            JavaLongAdder y = x;
            y.Value.Should().Be(10L);
        }

        [Theory]
        [
        InlineData(50000000, 64),
        InlineData(1000000, 64),
        InlineData(1000000, 32),
        InlineData(1000000, 24),
        InlineData(1000000, 16),
        InlineData(1000000, 8),
        InlineData(1000000, 4),
        InlineData(1000000, 2),
        InlineData(1000000, 1),
        ]
        public void ThreadLocalLongAdder_IsCorrectWithConcurrency(long total, int threadCount)
        {
            var value = new ThreadLocalLongAdder();
            var thread = new List<Thread>();

            for (int i = 0; i < threadCount; i++)
            {
                thread.Add(new Thread(() =>
                {
                    for (long j = 0; j < total; j++)
                    {
                        value.Increment();
                    }
                }));
            }

            thread.ForEach(t => t.Start());
            thread.ForEach(t => t.Join());
            value.Value.Should().Be(total * threadCount);
        }
    }
}
