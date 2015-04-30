using System;
using FluentAssertions;
using Metrics.Core;
using Xunit;

namespace Metrics.Tests.Metrics
{
    public class GaugeMetricTests
    {
        [Fact]
        public void GaugeMetric_ReportsNanOnException()
        {
            new FunctionGauge(() =>
            {
                throw new InvalidOperationException("test");
            }).Value.Should().Be(double.NaN);

            new DerivedGauge(new FunctionGauge(() => 5.0), (d) =>
            {
                throw new InvalidOperationException("test");
            }).Value.Should().Be(double.NaN);
        }
    }
}
