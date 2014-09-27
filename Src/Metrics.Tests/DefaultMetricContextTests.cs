using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Metrics.Sampling;
using Xunit;

namespace Metrics.Tests
{
    public class DefaultMetricContextTests
    {
        private readonly MetricsContext context = new DefaultMetricsContext();

        public MetricsData CurrentData { get { return this.context.DataProvider.CurrentMetricsData; } }

        [Fact]
        public void ContextEmptyChildContextIsSameContext()
        {
            var child = context.Context(string.Empty);
            object.ReferenceEquals(context, child).Should().BeTrue();
            child = context.Context(null);
            object.ReferenceEquals(context, child).Should().BeTrue();
        }

        [Fact]
        public void ContextChildWithSameNameAreSameInstance()
        {
            var first = context.Context("test");
            var second = context.Context("test");

            object.ReferenceEquals(first, second).Should().BeTrue();
        }

        [Fact]
        public void ContextCanCreateSubcontext()
        {
            context.Context("test").Counter("counter", Unit.Requests);

            var counterValue = CurrentData.ChildMetrics.SelectMany(c => c.Counters).Single();

            counterValue.Name.Should().Be("counter");
        }

        [Fact]
        public void ContextMetricsArePresentInMetricsData()
        {
            var counter = context.Counter("test", Unit.Requests);

            counter.Increment();

            var counterValue = CurrentData.Counters.Single();

            counterValue.Name.Should().Be("test");
            counterValue.Unit.Should().Be(Unit.Requests);
            counterValue.Value.Should().Be(1);
        }

        [Fact]
        public void ContextRaisesShutdownEventOnMetricsDisable()
        {
            context.MonitorEvents();
            context.Advanced.CompletelyDisableMetrics();
            context.ShouldRaise("ContextShuttingDown");
        }

        [Fact]
        public void ContextRaisesShutdownEventOnDispose()
        {
            context.MonitorEvents();
            context.Dispose();
            context.ShouldRaise("ContextShuttingDown");
        }

        [Fact]
        public void ContextDataProviderReflectsNewMetrics()
        {
            var provider = context.DataProvider;

            context.Counter("test", Unit.Bytes).Increment();

            provider.CurrentMetricsData.Counters.Should().HaveCount(1);
            provider.CurrentMetricsData.Counters.Single().Name.Should().Be("test");
            provider.CurrentMetricsData.Counters.Single().Value.Should().Be(1L);
        }

        [Fact]
        public void ContextDataProviderReflectsChildContxts()
        {
            var provider = context.DataProvider;

            var counter = context
                .Context("test")
                .Counter("test", Unit.Bytes);

            counter.Increment();

            provider.CurrentMetricsData.ChildMetrics.Should().HaveCount(1);
            provider.CurrentMetricsData.ChildMetrics.Single().Counters.Should().HaveCount(1);
            provider.CurrentMetricsData.ChildMetrics.Single().Counters.Single().Value.Should().Be(1);

            counter.Increment();

            provider.CurrentMetricsData.ChildMetrics.Single().Counters.Single().Value.Should().Be(2);
        }

        [Fact]
        public void ContextDisabledChildContextDoesNotShowInData()
        {
            context.Context("test").Counter("test", Unit.Bytes).Increment();

            CurrentData.ChildMetrics.Single()
                .Counters.Single().Name.Should().Be("test");

            context.ShutdownContext("test");

            CurrentData.ChildMetrics.Should().BeEmpty();
        }

        public class CustomCounter : Counter, MetricValueProvider<long>
        {

            public void Increment() { }
            public void Increment(long value) { }
            public void Decrement() { }
            public void Decrement(long value) { }
            public void Reset() { }
            public long Value { get { return 10L; } }
        }

        [Fact]
        public void ContextCanRegisterCustomCounter()
        {
            var counter = context.Advanced.Counter("custom", Unit.Calls, () => new CustomCounter());
            counter.Should().BeOfType<CustomCounter>();
            counter.Increment();
            context.DataProvider.CurrentMetricsData.Counters.Single().Value.Should().Be(10L);
        }

        public class CustomReservoir : Reservoir
        {
            private readonly List<long> values = new List<long>();

            public int Size { get { return this.values.Count; } }
            public void Update(long value) { this.values.Add(value); }
            public Snapshot Snapshot
            {
                get { return new UniformSnapshot(this.values); }
            }

            public void Reset()
            {
                this.values.Clear();
            }

            public IEnumerable<long> Values { get { return this.values; } }
        }

        [Fact]
        public void ContextCanRegisterTimerWithCustomReservoir()
        {
            var reservoir = new CustomReservoir();
            var timer = context.Advanced.Timer("custom", Unit.Calls, () => (Reservoir)reservoir);

            timer.Record(10L, TimeUnit.Nanoseconds);

            reservoir.Size.Should().Be(1);
            reservoir.Values.Single().Should().Be(10L);
        }

        public class CustomHistogram : Histogram, MetricValueProvider<HistogramValue>
        {
            private readonly CustomReservoir reservoir = new CustomReservoir();
            public void Update(long value) { this.reservoir.Update(value); }
            public void Reset() { this.reservoir.Reset(); }

            public CustomReservoir Reservoir { get { return this.reservoir; } }

            public HistogramValue Value
            {
                get
                {
                    return new HistogramValue(this.reservoir.Size,
                        this.reservoir.Values.Last(), this.reservoir.Snapshot);
                }
            }
        }

        [Fact]
        public void ContextCanRegisterTimerWithCustomHistogram()
        {
            var histogram = new CustomHistogram();

            var timer = context.Advanced.Timer("custom", Unit.Calls, () => (Histogram)histogram);

            timer.Record(10L, TimeUnit.Nanoseconds);

            histogram.Reservoir.Size.Should().Be(1);
            histogram.Reservoir.Values.Single().Should().Be(10L);
        }

    }
}
