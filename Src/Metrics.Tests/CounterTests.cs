
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Metrics.Core;
using Xunit;

namespace Metrics.Tests
{
    public class CounterTests
    {
        [Fact]
        public void CounterStartsFromZero()
        {
            new CounterMetric().Value.Should().Be(0L);
        }

        [Fact]
        public void CounterCanIncrement()
        {
            var counter = new CounterMetric();
            counter.Increment();
            counter.Value.Should().Be(1L);
        }

        [Fact]
        public void CounterCanIncrementWithValue()
        {
            var counter = new CounterMetric();
            counter.Increment(32L);
            counter.Value.Should().Be(32L);
        }

        [Fact]
        public void CounterCanIncrementMultipleTimes()
        {
            var counter = new CounterMetric();
            counter.Increment();
            counter.Increment();
            counter.Increment();
            counter.Value.Should().Be(3L);
        }

        [Fact]
        public void CounterCanDecrement()
        {
            var counter = new CounterMetric();
            counter.Decrement();
            counter.Value.Should().Be(-1L);
        }

        [Fact]
        public void CounterCanDecrementWithValue()
        {
            var counter = new CounterMetric();
            counter.Decrement(32L);
            counter.Value.Should().Be(-32L);
        }

        [Fact]
        public void CounterCanDecrementMultipleTimes()
        {
            var counter = new CounterMetric();
            counter.Decrement();
            counter.Decrement();
            counter.Decrement();
            counter.Value.Should().Be(-3L);
        }

        [Fact]
        public void CounterCanBeIncrementedOnMultipleThreads()
        {
            const int threadCount = 16;
            const long iterations = 1000 * 100;

            var counter = new CounterMetric();

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
        public void CounterCanReset()
        {
            var counter = new CounterMetric();
            counter.Increment();
            counter.Value.Should().Be(1L);
            counter.Reset();
            counter.Value.Should().Be(0L);
        }
    }
}
