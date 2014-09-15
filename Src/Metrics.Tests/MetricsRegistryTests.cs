using System;
using FluentAssertions;
using Metrics.Core;
using Xunit;

namespace Metrics.Tests
{
    public class MetricsRegistryTests
    {
        private static void AddMetrics(LocalRegistry registry)
        {
            var name = "Test";
            registry.Gauge(name, () => new FunctionGauge(() => 0.0), Unit.Calls);
            registry.Counter(name, Unit.Calls);
            registry.Meter(name, Unit.Calls, TimeUnit.Seconds);
            registry.Histogram(name, Unit.Calls, SamplingType.FavourRecent);
            registry.Timer(name, Unit.Calls, SamplingType.FavourRecent, TimeUnit.Seconds, TimeUnit.Milliseconds);
        }

        [Fact]
        public void MetricsRegistryDowsNotThrowOnMetricsOfDifferentTypeWithSameName()
        {
            var registry = new LocalRegistry();
            ((Action)(() => AddMetrics(registry))).ShouldNotThrow();
        }

        [Fact]
        public void MetricsRegistyMetricsAddedAreVisibleInTheDataProvider()
        {
            var registry = new LocalRegistry();

            var provider = registry.DataProvider;

            provider.Counters.Should().BeEmpty();

            registry.Counter("test", Unit.Bytes);

            provider.Counters.Should().HaveCount(1);
        }

    }
}
