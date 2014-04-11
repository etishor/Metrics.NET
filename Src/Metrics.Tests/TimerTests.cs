using System;
using FluentAssertions;
using Metrics.Core;
using Metrics.Utils;
using Xunit;

namespace Metrics.Tests
{
    public class TimerTests
    {
        [Fact]
        public void TimerCanCount()
        {
            Timer timer = new TimerMetric();
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
            Timer timer = new TimerMetric();

            Action action = () => timer.Time(() => { throw new InvalidOperationException(); });

            action.ShouldThrow<InvalidOperationException>();

            timer.Value.Rate.Count.Should().Be(1);
        }

        [Fact]
        public void TimerCanTrackTime()
        {
            Clock.TestClock clock = new Clock.TestClock();
            Timer timer = new TimerMetric(SamplingType.LongTerm, clock);
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
            Clock.TestClock clock = new Clock.TestClock();
            Timer timer = new TimerMetric(SamplingType.LongTerm, clock);

            var context = timer.NewContext();            
            clock.Advance(TimeUnit.Milliseconds, 100);
            using (context) { }
            clock.Advance(TimeUnit.Milliseconds, 100);
            using (context) { }
            
            timer.Value.Histogram.Count.Should().Be(1);
            timer.Value.Histogram.Max.Should().Be(TimeUnit.Milliseconds.ToNanoseconds(100));
        }
    }
}
