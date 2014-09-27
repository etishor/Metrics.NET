using FluentAssertions;
using Metrics.Core;
using Xunit;

namespace Metrics.Tests.Metrics
{
    public class HistogramMetricTests
    {
        private readonly HistogramMetric histogram = new HistogramMetric();

        [Fact]
        public void HistogramMetric_CanCount()
        {
            histogram.Update(1L);
            histogram.Value.Count.Should().Be(1);
            histogram.Update(1L);
            histogram.Value.Count.Should().Be(2);
        }

        [Fact]
        public void HistogramMetric_CanReset()
        {
            histogram.Update(1L);
            histogram.Update(10L);

            histogram.Value.Count.Should().NotBe(0);
            histogram.Value.LastValue.Should().NotBe(0);
            histogram.Value.Median.Should().NotBe(0);

            histogram.Reset();

            histogram.Value.Count.Should().Be(0);
            histogram.Value.LastValue.Should().Be(0);
            histogram.Value.Median.Should().Be(0);
        }
    }
}
