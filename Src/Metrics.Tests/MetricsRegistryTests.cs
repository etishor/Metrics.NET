using System;
using FluentAssertions;
using Metrics.Core;
using Xunit;

namespace Metrics.Tests
{
    public class MetricsRegistryTests
    {
        private static void AddMetrics(DefaultMetricsContext context)
        {
            var name = "Test";
            context.Gauge(name, () => new FunctionGauge(() => 0.0), Unit.Calls);
            context.Counter(name, Unit.Calls);
            context.Meter(name, Unit.Calls, TimeUnit.Seconds);
            context.Histogram(name, Unit.Calls, SamplingType.FavourRecent);
            context.Timer(name, Unit.Calls, SamplingType.FavourRecent, TimeUnit.Seconds, TimeUnit.Milliseconds);
        }

        [Fact]
        public void MetricsContextDowsNotThrowOnMetricsOfDifferentTypeWithSameName()
        {
            var context = new DefaultMetricsContext();
            ((Action)(() => AddMetrics(context))).ShouldNotThrow();
        }

        [Fact]
        public void MetricsContextMetricsAddedAreVisibleInTheDataProvider()
        {
            var contexst = new DefaultMetricsContext();

            contexst.DataProvider.CurrentMetricsData.Counters.Should().BeEmpty();

            contexst.Counter("test", Unit.Bytes);

            contexst.DataProvider.CurrentMetricsData.Counters.Should().HaveCount(1);
        }

    }
}
