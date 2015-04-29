using System;
using System.Linq;
using FluentAssertions;
using Metrics.MetricData;
using Xunit;

namespace Metrics.Tests.Core
{
    public class DefaultContextTests
    {
        private readonly MetricsContext context = new DefaultMetricsContext();
        public MetricsData CurrentData { get { return this.context.DataProvider.CurrentMetricsData; } }

        [Fact]
        public void MetricsContext_EmptyChildContextIsSameContext()
        {
            var child = context.Context(string.Empty);
            object.ReferenceEquals(context, child).Should().BeTrue();
            child = context.Context(null);
            object.ReferenceEquals(context, child).Should().BeTrue();
        }

        [Fact]
        public void MetricsContext_ChildWithSameNameAreSameInstance()
        {
            var first = context.Context("test");
            var second = context.Context("test");

            object.ReferenceEquals(first, second).Should().BeTrue();
        }

        [Fact]
        public void MetricsContext_CanCreateSubcontext()
        {
            context.Context("test").Counter("counter", Unit.Requests);

            var counterValue = CurrentData.ChildMetrics.SelectMany(c => c.Counters).Single();

            counterValue.Name.Should().Be("counter");
        }

        [Fact]
        public void MetricsContext_MetricsArePresentInMetricsData()
        {
            var counter = context.Counter("test", Unit.Requests);

            counter.Increment();

            var counterValue = CurrentData.Counters.Single();

            counterValue.Name.Should().Be("test");
            counterValue.Unit.Should().Be(Unit.Requests);
            counterValue.Value.Count.Should().Be(1);
        }

        [Fact]
        public void MetricsContext_RaisesShutdownEventOnMetricsDisable()
        {
            context.MonitorEvents();
            context.Advanced.CompletelyDisableMetrics();
            context.ShouldRaise("ContextShuttingDown");
        }

        [Fact]
        public void MetricsContext_RaisesShutdownEventOnDispose()
        {
            context.MonitorEvents();
            context.Dispose();
            context.ShouldRaise("ContextShuttingDown");
        }

        [Fact]
        public void MetricsContext_DataProviderReflectsNewMetrics()
        {
            var provider = context.DataProvider;

            context.Counter("test", Unit.Bytes).Increment();

            provider.CurrentMetricsData.Counters.Should().HaveCount(1);
            provider.CurrentMetricsData.Counters.Single().Name.Should().Be("test");
            provider.CurrentMetricsData.Counters.Single().Value.Count.Should().Be(1L);
        }

        [Fact]
        public void MetricsContext_DataProviderReflectsChildContxts()
        {
            var provider = context.DataProvider;

            var counter = context
                .Context("test")
                .Counter("test", Unit.Bytes);

            counter.Increment();

            provider.CurrentMetricsData.ChildMetrics.Should().HaveCount(1);
            provider.CurrentMetricsData.ChildMetrics.Single().Counters.Should().HaveCount(1);
            provider.CurrentMetricsData.ChildMetrics.Single().Counters.Single().Value.Count.Should().Be(1);

            counter.Increment();

            provider.CurrentMetricsData.ChildMetrics.Single().Counters.Single().Value.Count.Should().Be(2);
        }

        [Fact]
        public void MetricsContext_DisabledChildContextDoesNotShowInData()
        {
            context.Context("test").Counter("test", Unit.Bytes).Increment();

            CurrentData.ChildMetrics.Single()
                .Counters.Single().Name.Should().Be("test");

            context.ShutdownContext("test");

            CurrentData.ChildMetrics.Should().BeEmpty();
        }

        [Fact]
        public void MetricsContext_DowsNotThrowOnMetricsOfDifferentTypeWithSameName()
        {
            ((Action)(() =>
            {
                var name = "Test";
                context.Gauge(name, () => 0.0, Unit.Calls);
                context.Counter(name, Unit.Calls);
                context.Meter(name, Unit.Calls, TimeUnit.Seconds);
                context.Histogram(name, Unit.Calls, SamplingType.FavourRecent);
                context.Timer(name, Unit.Calls, SamplingType.FavourRecent, TimeUnit.Seconds, TimeUnit.Milliseconds);
            })).ShouldNotThrow();
        }

        [Fact]
        public void MetricsContext_MetricsAddedAreVisibleInTheDataProvider()
        {
            context.DataProvider.CurrentMetricsData.Counters.Should().BeEmpty();
            context.Counter("test", Unit.Bytes);
            context.DataProvider.CurrentMetricsData.Counters.Should().HaveCount(1);
        }

        [Fact]
        public void MetricsContext_CanPropagateValueTags()
        {
            context.Counter("test", Unit.None, "tag");
            context.DataProvider.CurrentMetricsData.Counters.Single().Tags.Should().Equal(new[] { "tag" });

            context.Meter("test", Unit.None, tags: "tag");
            context.DataProvider.CurrentMetricsData.Meters.Single().Tags.Should().Equal(new[] { "tag" });

            context.Histogram("test", Unit.None, tags: "tag");
            context.DataProvider.CurrentMetricsData.Histograms.Single().Tags.Should().Equal(new[] { "tag" });

            context.Timer("test", Unit.None, tags: "tag");
            context.DataProvider.CurrentMetricsData.Timers.Single().Tags.Should().Equal(new[] { "tag" });
        }

        [Fact]
        public void MetricsContext_MergeContextMergesNewCounters()
        {
            var primary = new DefaultMetricsContext();
            var secondary = new DefaultMetricsContext();

            secondary.Counter("NewCounter", Unit.Bytes, new MetricTags());
            primary.MergeContext(secondary, true);

            primary.DataProvider.CurrentMetricsData.Counters.Single().Name.Should().Be("NewCounter");
            primary.DataProvider.CurrentMetricsData.Counters.Single().Value.Count.Should().Be(0);
            secondary.DataProvider.CurrentMetricsData.Counters.Should().BeEmpty();
        }

        [Fact]
        public void MetricsContext_MergeContextMergesExistingCounters()
        {
            var primary = new DefaultMetricsContext();
            var secondary = new DefaultMetricsContext();

            primary.Counter("NewCounter", Unit.Bytes, new MetricTags()).Increment(5);
            secondary.Counter("NewCounter", Unit.Bytes, new MetricTags()).Increment(10);
            primary.MergeContext(secondary, false);

            primary.DataProvider.CurrentMetricsData.Counters.Single().Name.Should().Be("NewCounter");
            primary.DataProvider.CurrentMetricsData.Counters.Single().Value.Count.Should().Be(15);

            secondary.DataProvider.CurrentMetricsData.Counters.Should().NotBeEmpty();
            secondary.DataProvider.CurrentMetricsData.Counters.Single().Value.Count.Should().Be(10);
        }

        [Fact]
        public void MetricsContext_MergeContextMergesNewTimers()
        {
            var primary = new DefaultMetricsContext();
            var secondary = new DefaultMetricsContext();

            secondary.Timer("NewTimer", Unit.Events, SamplingType.FavourRecent, TimeUnit.Seconds, TimeUnit.Seconds, new MetricTags()).Record(34, TimeUnit.Seconds);
            primary.MergeContext(secondary, true);

            primary.DataProvider.CurrentMetricsData.Timers.Single().Name.Should().Be("NewTimer");
            primary.DataProvider.CurrentMetricsData.Timers.Single().Value.Histogram.Mean.Should().BeApproximately(34, 0.0001);

            secondary.DataProvider.CurrentMetricsData.Timers.Should().BeEmpty();
        }

        [Fact]
        public void MetricsContext_MergeContextMergesExistingTimers()
        {
            var primary = new DefaultMetricsContext();
            var secondary = new DefaultMetricsContext();

            primary.Timer("NewTimer", Unit.Events, SamplingType.FavourRecent, TimeUnit.Seconds, TimeUnit.Seconds, new MetricTags()).Record(10, TimeUnit.Seconds);
            primary.Timer("NewTimer", Unit.Events, SamplingType.FavourRecent, TimeUnit.Seconds, TimeUnit.Seconds, new MetricTags()).Record(30, TimeUnit.Seconds);
            secondary.Timer("NewTimer", Unit.Events, SamplingType.FavourRecent, TimeUnit.Seconds, TimeUnit.Seconds, new MetricTags()).Record(40, TimeUnit.Seconds);
            secondary.Timer("NewTimer", Unit.Events, SamplingType.FavourRecent, TimeUnit.Seconds, TimeUnit.Seconds, new MetricTags()).Record(40, TimeUnit.Seconds);
            primary.MergeContext(secondary, false);

            primary.DataProvider.CurrentMetricsData.Timers.Single().Name.Should().Be("NewTimer");
            primary.DataProvider.CurrentMetricsData.Timers.Single().Value.Histogram.Mean.Should().BeApproximately(30, 0.0001);

            secondary.DataProvider.CurrentMetricsData.Timers.Should().NotBeEmpty();
        }

        [Fact]
        public void MetricsContext_MergeContextMergesNewMeters()
        {
            var primary = new DefaultMetricsContext();
            var secondary = new DefaultMetricsContext();

            secondary.Meter("NewMeter", Unit.Events, TimeUnit.Seconds, new MetricTags());
            primary.MergeContext(secondary, true);

            primary.DataProvider.CurrentMetricsData.Meters.Single().Name.Should().Be("NewMeter");
            primary.DataProvider.CurrentMetricsData.Meters.Single().Value.MeanRate.Should().BeApproximately(0, 0.0001);

            secondary.DataProvider.CurrentMetricsData.Meters.Should().BeEmpty();
        }

        [Fact(Skip = "Merging will probably be removed")]
        public void MetricsContext_MergeContextMergesExistingMeters()
        {
            var primary = new DefaultMetricsContext();
            var secondary = new DefaultMetricsContext();

            primary.Meter("NewMeter", Unit.Events, TimeUnit.Seconds, new MetricTags());
            secondary.Meter("NewMeter", Unit.Events, TimeUnit.Seconds, new MetricTags()).Mark();

            primary.DataProvider.CurrentMetricsData.Meters.Single().Value.MeanRate.Should().BeApproximately(0, 0.0001);
            primary.MergeContext(secondary, false);

            primary.DataProvider.CurrentMetricsData.Meters.Single().Name.Should().Be("NewMeter");
            primary.DataProvider.CurrentMetricsData.Meters.Single().Value.MeanRate.Should().NotBe(0);

            secondary.DataProvider.CurrentMetricsData.Meters.Should().NotBeEmpty();
        }

        [Fact]
        public void MetricsContext_MergeContextMergesNewHistograms()
        {
            var primary = new DefaultMetricsContext();
            var secondary = new DefaultMetricsContext();

            secondary.Histogram("NewHistogram", Unit.Events, SamplingType.FavourRecent, new MetricTags()).Update(23);
            primary.MergeContext(secondary, true);

            primary.DataProvider.CurrentMetricsData.Histograms.Single().Name.Should().Be("NewHistogram");
            primary.DataProvider.CurrentMetricsData.Histograms.Single().Value.Mean.Should().BeApproximately(23, 0.0001);

            secondary.DataProvider.CurrentMetricsData.Histograms.Should().BeEmpty();
        }

        [Fact]
        public void MetricsContext_MergeContextMergesExistingHistograms()
        {
            var primary = new DefaultMetricsContext();
            var secondary = new DefaultMetricsContext();

            primary.Histogram("NewHistogram", Unit.Events, SamplingType.FavourRecent, new MetricTags()).Update(12);
            secondary.Histogram("NewHistogram", Unit.Events, SamplingType.FavourRecent, new MetricTags()).Update(24);
            primary.MergeContext(secondary, false);

            primary.DataProvider.CurrentMetricsData.Histograms.Single().Name.Should().Be("NewHistogram");
            primary.DataProvider.CurrentMetricsData.Histograms.Single().Value.Mean.Should().BeApproximately(18, 0.0001);

            secondary.DataProvider.CurrentMetricsData.Histograms.Should().NotBeEmpty();
        }

        [Fact]
        public void MetricsContext_MergeContextMergesNewGauges()
        {
            var primary = new DefaultMetricsContext();
            var secondary = new DefaultMetricsContext();

            secondary.Gauge("NewGauge", () => 33.34, Unit.KiloBytes, new MetricTags());
            primary.MergeContext(secondary, true);

            primary.DataProvider.CurrentMetricsData.Gauges.Single().Name.Should().Be("NewGauge");
            primary.DataProvider.CurrentMetricsData.Gauges.Single().Value.Should().BeApproximately(33.34, 0.0001);

            secondary.DataProvider.CurrentMetricsData.Gauges.Should().BeEmpty();
        }

        [Fact]
        public void MetricsContext_MergeContextMergesExistingGauges()
        {
            var primary = new DefaultMetricsContext();
            var secondary = new DefaultMetricsContext();
            var third = new DefaultMetricsContext();

            primary.Gauge("NewGauge", () => 30, Unit.KiloBytes, new MetricTags());
            secondary.Gauge("NewGauge", () => 1080, Unit.KiloBytes, new MetricTags());
            primary.DataProvider.CurrentMetricsData.Gauges.Single().Value.Should().BeApproximately(30, 0.0001);
            primary.MergeContext(secondary, false);

            third.Gauge("NewGauge", () => 97.31, Unit.KiloBytes, new MetricTags());
            primary.MergeContext(third, true);

            // make sure there's one and it uses our median
            primary.DataProvider.CurrentMetricsData.Gauges.Single().Name.Should().Be("NewGauge");
            primary.DataProvider.CurrentMetricsData.Gauges.Single().Value.Should().BeApproximately(97.31, 0.0001);

            secondary.DataProvider.CurrentMetricsData.Gauges.Should().NotBeEmpty();
        }

        [Fact]
        public void MetricsContext_MergeContextMergesChildren()
        {
            var primary = new DefaultMetricsContext();
            var pchild = new DefaultMetricsContext();

            var secondary = new DefaultMetricsContext();
            var schild1 = new DefaultMetricsContext();
            var schild2 = new DefaultMetricsContext();

            pchild.Counter("Child1", Unit.Calls, new MetricTags()).Increment(5);
            primary.AttachContext("C1", pchild);

            schild1.Counter("Child1", Unit.Calls, new MetricTags()).Increment(15);
            schild2.Counter("Child2", Unit.Calls, new MetricTags()).Increment(10);
            secondary.AttachContext("C1", schild1);
            secondary.AttachContext("C2", schild2);

            primary.MergeContext(secondary, false);

            // make sure there's one and it uses our median
            primary.Context("C1").Should().NotBeNull();
            primary.Context("C1").DataProvider.CurrentMetricsData.Counters.Single().Value.Count.Should().Be(20);
            primary.Context("C2").DataProvider.CurrentMetricsData.Counters.Single().Value.Count.Should().Be(10);
        }
    }
}
