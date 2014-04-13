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
    }
}
