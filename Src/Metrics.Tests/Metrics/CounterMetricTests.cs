
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Metrics.Core;
using Xunit;

namespace Metrics.Tests.Metrics
{
    public class CounterMetricTests
    {
        private readonly CounterMetric counter = new CounterMetric();

        [Fact]
        public void CounterMetric_StartsFromZero()
        {
            counter.Value.Should().Be(0L);
        }

        [Fact]
        public void CounterMetric_CanIncrement()
        {
            counter.Increment();
            counter.Value.Should().Be(1L);
        }

        [Fact]
        public void CounterMetric_CanIncrementWithValue()
        {
            counter.Increment(32L);
            counter.Value.Should().Be(32L);
        }

        [Fact]
        public void CounterMetric_CanIncrementMultipleTimes()
        {
            counter.Increment();
            counter.Increment();
            counter.Increment();
            counter.Value.Should().Be(3L);
        }

        [Fact]
        public void CounterMetric_CanDecrement()
        {
            counter.Decrement();
            counter.Value.Should().Be(-1L);
        }

        [Fact]
        public void CounterMetric_CanDecrementWithValue()
        {
            counter.Decrement(32L);
            counter.Value.Should().Be(-32L);
        }

        [Fact]
        public void CounterMetric_CanDecrementMultipleTimes()
        {
            counter.Decrement();
            counter.Decrement();
            counter.Decrement();
            counter.Value.Should().Be(-3L);
        }

        [Fact]
        public void CounterMetric_CanBeIncrementedOnMultipleThreads()
        {
            const int threadCount = 16;
            const long iterations = 1000 * 100;

            List<Thread> threads = new List<Thread>();
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();

            for (int i = 0; i < threadCount; i++)
            {
                threads.Add(new Thread(s =>
                {
                    tcs.Task.Wait();
                    for (long j = 0; j < iterations; j++)
                    {
                        counter.Increment();
                    }
                }));
            }

            threads.ForEach(t => t.Start());
            tcs.SetResult(0);
            threads.ForEach(t => t.Join());

            counter.Value.Should().Be(threadCount * iterations);
        }

        [Fact]
        public void CounterMetric_CanReset()
        {
            counter.Increment();
            counter.Value.Should().Be(1L);
            counter.Reset();
            counter.Value.Should().Be(0L);
        }
    }
}
