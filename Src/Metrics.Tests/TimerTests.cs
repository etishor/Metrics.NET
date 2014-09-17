using System;
using FluentAssertions;
using Metrics.Core;
using Metrics.Tests.TestUtils;
using Metrics.Utils;
using Xunit;

namespace Metrics.Tests
{
    public class TimerTests
    {
        private readonly TestClock clock = new TestClock();
        private readonly TestScheduler scheduler;
        private readonly TimerMetric timer;

        public TimerTests()
        {
            this.scheduler = new TestScheduler(clock);
            this.timer = new TimerMetric(SamplingType.LongTerm, new MeterMetric(clock, scheduler), clock);
        }

        [Fact]
        public void TimerCanCount()
        {
            TimerMetric timer = new TimerMetric();
            timer.Value.Rate.Count.Should().Be(0);
            using (timer.NewContext()) { }
            timer.Value.Rate.Count.Should().Be(1);
            using (timer.NewContext()) { }
            timer.Value.Rate.Count.Should().Be(2);
            timer.Time(() => { });
            timer.Value.Rate.Count.Should().Be(3);
            timer.Time(() => 1);
            timer.Value.Rate.Count.Should().Be(4);
        }

        [Fact]
        public void TimerCountsEvenIfActionThrows()
        {
            Action action = () => this.timer.Time(() => { throw new InvalidOperationException(); });

            action.ShouldThrow<InvalidOperationException>();

            this.timer.Value.Rate.Count.Should().Be(1);
        }

        [Fact]
        public void TimerCanTrackTime()
        {
            using (timer.NewContext())
            {
                clock.Advance(TimeUnit.Milliseconds, 100);
            }

            timer.Value.Histogram.Count.Should().Be(1);
            timer.Value.Histogram.Max.Should().Be(TimeUnit.Milliseconds.ToNanoseconds(100));
        }

        [Fact]
        public void TimerContextRecordsTimeOnlyOnFirstDispose()
        {
            var context = timer.NewContext();
            clock.Advance(TimeUnit.Milliseconds, 100);
            using (context) { }
            clock.Advance(TimeUnit.Milliseconds, 100);
            using (context) { }

            timer.Value.Histogram.Count.Should().Be(1);
            timer.Value.Histogram.Max.Should().Be(TimeUnit.Milliseconds.ToNanoseconds(100));
        }

        [Fact]
        public void TimerContextReportsElapsedTime()
        {
            using (var context = timer.NewContext())
            {
                clock.Advance(TimeUnit.Milliseconds, 100);
                context.Elapsed.TotalMilliseconds.Should().Be(100);
            }
        }

        [Fact]
        public void TimerCanReset()
        {
            using (var context = timer.NewContext())
            {
                clock.Advance(TimeUnit.Milliseconds, 100);
            }

            timer.Value.Rate.Count.Should().NotBe(0);
            timer.Value.Histogram.Count.Should().NotBe(0);

            timer.Reset();

            timer.Value.Rate.Count.Should().Be(0);
            timer.Value.Histogram.Count.Should().Be(0);
        }
    }
}
