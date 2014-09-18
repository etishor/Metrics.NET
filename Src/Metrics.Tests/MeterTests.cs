
using FluentAssertions;
using Metrics.Core;
using Metrics.Tests.TestUtils;
using Xunit;

namespace Metrics.Tests
{
    public class MeterTests
    {
        [Fact]
        public void MeterCanCount()
        {
            var meter = new MeterMetric();
            meter.Mark();

            meter.Value.Count.Should().Be(1L);

            meter.Mark();
            meter.Value.Count.Should().Be(2L);

            meter.Mark(8L);
            meter.Value.Count.Should().Be(10L);
        }

        [Fact]
        public void MeterCanCalculateMeanRate()
        {
            TestClock clock = new TestClock();
            TestScheduler scheduler = new TestScheduler(clock);

            var meter = new MeterMetric(clock, scheduler);

            meter.Mark();
            clock.Advance(TimeUnit.Seconds, 1);

            meter.Value.MeanRate.Should().Be(1);

            clock.Advance(TimeUnit.Seconds, 1);

            meter.Value.MeanRate.Should().Be(0.5);
        }

        [Fact]
        public void MeterStartsAtZero()
        {
            var meter = new MeterMetric();
            meter.Value.MeanRate.Should().Be(0L);
            meter.Value.OneMinuteRate.Should().Be(0L);
            meter.Value.FiveMinuteRate.Should().Be(0L);
            meter.Value.FifteenMinuteRate.Should().Be(0L);
        }

        [Fact]
        public void MeterCanComputeRates()
        {
            TestClock clock = new TestClock();
            TestScheduler scheduler = new TestScheduler(clock);

            var meter = new MeterMetric(clock, scheduler);

            meter.Mark();
            clock.Advance(TimeUnit.Seconds, 10);
            meter.Mark(2);

            var value = meter.Value;

            value.MeanRate.Should().BeApproximately(0.3, 0.001);
            value.OneMinuteRate.Should().BeApproximately(0.1840, 0.001);
            value.FiveMinuteRate.Should().BeApproximately(0.1966, 0.001);
            value.FifteenMinuteRate.Should().BeApproximately(0.1988, 0.001);
        }

        [Fact]
        public void MeterCanReset()
        {
            TestClock clock = new TestClock();
            TestScheduler scheduler = new TestScheduler(clock);

            var meter = new MeterMetric(clock, scheduler);

            meter.Mark();
            meter.Mark();
            clock.Advance(TimeUnit.Seconds, 10);
            meter.Mark(2);

            meter.Reset();
            meter.Value.Count.Should().Be(0L);
            meter.Value.OneMinuteRate.Should().Be(0);
            meter.Value.FiveMinuteRate.Should().Be(0);
            meter.Value.FifteenMinuteRate.Should().Be(0);
        }
    }
}
