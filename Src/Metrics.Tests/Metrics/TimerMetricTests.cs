using System;
using FluentAssertions;
using Metrics.Core;
using Metrics.Utils;
using Xunit;

namespace Metrics.Tests.Metrics
{
    public class TimerMetricTests
    {
        private readonly TestClock clock = new TestClock();
        private readonly TestScheduler scheduler;
        private readonly TimerMetric timer;

        public TimerMetricTests()
        {
            this.scheduler = new TestScheduler(clock);
            this.timer = new TimerMetric(SamplingType.FavourRecent, new MeterMetric(clock, scheduler), clock);
        }

        [Fact]
        public void TimerMetric_CanCount()
        {
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
        public void TimerMetric_CountsEvenIfActionThrows()
        {
            Action action = () => this.timer.Time(() => { throw new InvalidOperationException(); });

            action.ShouldThrow<InvalidOperationException>();

            this.timer.Value.Rate.Count.Should().Be(1);
        }

        [Fact]
        public void TimerMetric_CanTrackTime()
        {
            using (timer.NewContext())
            {
                clock.Advance(TimeUnit.Milliseconds, 100);
            }

            timer.Value.Histogram.Count.Should().Be(1);
            timer.Value.Histogram.Max.Should().Be(TimeUnit.Milliseconds.ToNanoseconds(100));
        }

        [Fact]
        public void TimerMetric_ContextRecordsTimeOnlyOnFirstDispose()
        {
            var context = timer.NewContext();
            clock.Advance(TimeUnit.Milliseconds, 100);
            context.Dispose(); // passing the structure to using() creates a copy
            clock.Advance(TimeUnit.Milliseconds, 100);
            context.Dispose();

            timer.Value.Histogram.Count.Should().Be(1);
            timer.Value.Histogram.Max.Should().Be(TimeUnit.Milliseconds.ToNanoseconds(100));
        }

        [Fact]
        public void TimerMetric_ContextReportsElapsedTime()
        {
            using (var context = timer.NewContext())
            {
                clock.Advance(TimeUnit.Milliseconds, 100);
                context.Elapsed.TotalMilliseconds.Should().Be(100);
            }
        }

        [Fact]
        public void TimerMetric_CanReset()
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

        [Fact]
        public void TimerMetric_RecordsUserValue()
        {
            timer.Record(1L, TimeUnit.Milliseconds, "A");
            timer.Record(10L, TimeUnit.Milliseconds, "B");

            timer.Value.Histogram.MinUserValue.Should().Be("A");
            timer.Value.Histogram.MaxUserValue.Should().Be("B");
        }

        [Fact]
        public void TimerMetric_RecordsActiveSessions()
        {
            timer.Value.ActiveSessions.Should().Be(0);
            var context1 = timer.NewContext();
            timer.Value.ActiveSessions.Should().Be(1);
            var context2 = timer.NewContext();
            timer.Value.ActiveSessions.Should().Be(2);
            context1.Dispose();
            timer.Value.ActiveSessions.Should().Be(1);
            context2.Dispose();
            timer.Value.ActiveSessions.Should().Be(0);
        }
    }
}
