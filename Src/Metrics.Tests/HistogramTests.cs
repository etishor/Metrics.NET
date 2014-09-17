using FluentAssertions;
using Metrics.Core;
using Xunit;

namespace Metrics.Tests
{
    public class HistogramTests
    {
        [Fact]
        public void HistogramCanCount()
        {
            var histogram = new HistogramMetric(new UniformReservoir());
            histogram.Update(1L);
            histogram.Value.Count.Should().Be(1);
            histogram.Update(1L);
            histogram.Value.Count.Should().Be(2);
        }

        [Fact]
        public void HistogramCanReset()
        {
            var histogram = new HistogramMetric();
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
