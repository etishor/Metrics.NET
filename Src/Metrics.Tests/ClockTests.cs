using System.Threading;
using FluentAssertions;
using Metrics.Utils;
using Xunit;

namespace Metrics.Tests
{
    public class ClockTests
    {
        [Fact]
        public void ClockDefaultCanMeasureTime()
        {
            var start = Clock.Default.Nanoseconds;
            Thread.Sleep(20);
            var end = Clock.Default.Nanoseconds;
            var elapsed = TimeUnit.Nanoseconds.ToMilliseconds(end - start);
            elapsed.Should().BeInRange(18, 22);
        }

        [Fact]
        public void ClockSystemCanMeasureTime()
        {
            var start = Clock.SystemDateTime.Nanoseconds;
            Thread.Sleep(20);
            var end = Clock.SystemDateTime.Nanoseconds;
            var elapsed = TimeUnit.Nanoseconds.ToMilliseconds(end - start);
            elapsed.Should().BeInRange(18, 22);
        }
    }
}
