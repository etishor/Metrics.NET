using System.Linq;
using FluentAssertions;
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
            context.CompletelyDisableMetrics();
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

    }
}
