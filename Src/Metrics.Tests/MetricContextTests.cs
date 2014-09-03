using System.Linq;
using FluentAssertions;
using Xunit;

namespace Metrics.Tests
{
    public class MetricContextTests
    {
        [Fact]
        public void ContextEmptyChildContextIsSameContext()
        {
            MetricsContext context = new DefaultMetricsContext();

            var child = context.Context(string.Empty);

            object.ReferenceEquals(context, child).Should().BeTrue();

            child = context.Context(null);

            object.ReferenceEquals(context, child).Should().BeTrue();
        }

        [Fact]
        public void ContextCanCreateSubcontext()
        {
            MetricsContext context = new DefaultMetricsContext();

            context.Context("test").Counter("counter", Unit.Requests);

            var counterValue = context.DataProvider.CurrentMetricsData.ChildMetrics.SelectMany(c => c.Counters).Single();

            counterValue.Name.Should().Be("counter");
        }

        [Fact]
        public void ContextMetricsArePresentInMetricsData()
        {
            MetricsContext context = new DefaultMetricsContext();

            var counter = context.Counter("test", Unit.Requests);

            counter.Increment();

            var counterValue = context.DataProvider.CurrentMetricsData.Counters.Single();

            counterValue.Name.Should().Be("test");
            counterValue.Unit.Should().Be(Unit.Requests);
            counterValue.Value.Should().Be(1);
        }

        [Fact]
        public void ContextRaisesShutdownEventOnMetricsDisable()
        {
            MetricsContext context = new DefaultMetricsContext();
            context.MonitorEvents();
            context.CompletelyDisableMetrics();
            context.ShouldRaise("ContextShuttingDown");
        }

        [Fact]
        public void ContextRaisesShutdownEventOnDispose()
        {
            MetricsContext context = new DefaultMetricsContext();
            context.MonitorEvents();
            context.Dispose();
            context.ShouldRaise("ContextShuttingDown");
        }

        [Fact]
        public void ContextDataProviderReflectsNewMetrics()
        {
            MetricsContext context = new DefaultMetricsContext();

            var provider = context.DataProvider;

            context.Counter("test", Unit.Bytes).Increment();

            provider.CurrentMetricsData.Counters.Should().HaveCount(1);
            provider.CurrentMetricsData.Counters.Single().Name.Should().Be("test");
            provider.CurrentMetricsData.Counters.Single().Value.Should().Be(1L);
        }

        [Fact]
        public void ContextDisabledChildContextDoesNotShowInData()
        {
            MetricsContext context = new DefaultMetricsContext();

            context.Context("test").Counter("test", Unit.Bytes).Increment();

            context.DataProvider.CurrentMetricsData.ChildMetrics.Single()
                .Counters.Single().Name.Should().Be("test");

            context.ShutdownContext("test");

            context.DataProvider.CurrentMetricsData.ChildMetrics.Should().BeEmpty();
        }

    }
}
