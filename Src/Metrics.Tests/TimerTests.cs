using System;
using FluentAssertions;
using Metrics.Core;
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
            var x = timer.Time(() => 1);
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


    }
}
