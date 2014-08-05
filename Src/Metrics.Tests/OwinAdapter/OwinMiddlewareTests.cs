using FluentAssertions;
using Xunit;

namespace Metrics.Tests.OwinAdapter
{
    public class OwinMiddlewareTests : IUseFixture<OwinMetricsTestData>
    {
        private OwinMetricsTestData data;

        [Fact]
        public void ShouldBeAbleToRecordErrors()
        {
            this.data.MeterMetric.Value.Count.Should().Be(data.ExpectedResults.ErrorCount);
        }

        [Fact]
        public void ShouldBeAbleToRecordActiveRequestCounts()
        {
            this.data.CounterMetric.Value.Should().Be(data.ExpectedResults.RequestCount);
        }

        [Fact]
        public void ShouldRecordHistogramMetricsForPostSizeAndTimePerRequest()
        {
            this.data.TimerMetric.Value.Rate.Count.Should().Be(data.ExpectedResults.TimerRateCount);
            this.data.TimerMetric.Value.Histogram.Count.Should().Be(data.ExpectedResults.HistogramCount);
            this.data.TimerMetric.Value.Histogram.Max.Should().BeGreaterThan(0);
            this.data.TimerMetric.Value.Histogram.Min.Should().BeGreaterThan(0);
            this.data.TimerMetric.Value.Histogram.Mean.Should().BeGreaterThan(0);

        }

        public void SetFixture(OwinMetricsTestData metricsTestData)
        {
            this.data = metricsTestData;
        }
    }
}
