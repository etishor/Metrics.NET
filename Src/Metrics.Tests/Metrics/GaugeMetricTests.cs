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

        [Fact]
        public void GaugeMetric_MergingGaugesReturnsMedian()
        {
            var gauge = new FunctionGauge(() =>
            {
                return 2.5;
            });

            // test with 1 values
            gauge.Value.Should().Be(2.5);

            gauge.Merge(new FunctionGauge(() =>
            {
                return 5.0;
            }));

            // test with 2 values
            gauge.Value.Should().Be(2.5);

            gauge.Merge(new FunctionGauge(() =>
            {
                return 7.5;
            }));

            // test with 3 values
            gauge.Value.Should().Be(5.0);

            gauge.Merge(new FunctionGauge(() =>
            {
                return 10.0;
            }));

            gauge.Merge(new FunctionGauge(() =>
            {
                return 1025.0;
            }));

            // test with 5 values
            gauge.Value.Should().Be(7.5);

            gauge.Merge(new FunctionGauge(() =>
            {
                return 5000.0;
            }));

            gauge.Merge(new FunctionGauge(() =>
            {
                return 98000.0;
            }));

            // test with 7 values
            gauge.Value.Should().Be(10.0);
        }
    }
}
