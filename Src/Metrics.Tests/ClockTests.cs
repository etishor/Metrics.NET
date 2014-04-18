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
            var startDefault = Clock.Default.Nanoseconds;
            var startSystem = Clock.SystemDateTime.Nanoseconds;
            Thread.Sleep(20);
            var endDefault = Clock.Default.Nanoseconds;
            var endSystem = Clock.SystemDateTime.Nanoseconds;

            var elapsedDefault = TimeUnit.Nanoseconds.ToMilliseconds(endDefault - startDefault);
            var elapsedSystem = TimeUnit.Nanoseconds.ToMilliseconds(endSystem - startSystem);

            ((double)elapsedDefault).Should().BeApproximately(elapsedSystem, 10);
        }
    }
}
