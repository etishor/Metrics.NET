using FluentAssertions;
using Metrics.Utils;
using Xunit;

namespace Metrics.Tests.Utils
{
    public class TimeUnitTests
    {
        [Fact]
        public void TimeUnit_ConvertsToZeroOnFractionalUnit()
        {
            TimeUnit.Seconds.ToMinutes(30L).Should().Be(0);
        }

        [Fact]
        public void TimeUnit_CanConvertBetweenUnits()
        {
            TimeUnit.Nanoseconds.ToNanoseconds(10L).Should().Be(10L);
            TimeUnit.Nanoseconds.ToMicroseconds(10L * 1000L).Should().Be(10L);
            TimeUnit.Nanoseconds.ToMilliseconds(10L * 1000L * 1000L).Should().Be(10L);
            TimeUnit.Nanoseconds.ToSeconds(10L * 1000L * 1000L * 1000L).Should().Be(10L);
            TimeUnit.Nanoseconds.ToMinutes(10L * 1000L * 1000L * 1000L * 60L).Should().Be(10L);
            TimeUnit.Nanoseconds.ToHours(10L * 1000L * 1000L * 1000L * 60L * 60L).Should().Be(10L);
            TimeUnit.Nanoseconds.ToDays(10L * 1000L * 1000L * 1000L * 60L * 60L * 24L).Should().Be(10L);

            TimeUnit.Microseconds.ToNanoseconds(10L).Should().Be(10L * 1000L);
            TimeUnit.Microseconds.ToMicroseconds(10L).Should().Be(10L);
            TimeUnit.Microseconds.ToMilliseconds(10L * 1000L).Should().Be(10L);
            TimeUnit.Microseconds.ToSeconds(10L * 1000L * 1000L).Should().Be(10L);
            TimeUnit.Microseconds.ToMinutes(10L * 1000L * 1000L * 60L).Should().Be(10L);
            TimeUnit.Microseconds.ToHours(10L * 1000L * 1000L * 60L * 60L).Should().Be(10L);
            TimeUnit.Microseconds.ToDays(10L * 1000L * 1000L * 60L * 60L * 24L).Should().Be(10L);

            TimeUnit.Milliseconds.ToNanoseconds(10L).Should().Be(10L * 1000L * 1000L);
            TimeUnit.Milliseconds.ToMicroseconds(10L).Should().Be(10L * 1000L);
            TimeUnit.Milliseconds.ToMilliseconds(10L).Should().Be(10L);
            TimeUnit.Milliseconds.ToSeconds(10L * 1000L).Should().Be(10L);
            TimeUnit.Milliseconds.ToMinutes(10L * 1000L * 60L).Should().Be(10L);
            TimeUnit.Milliseconds.ToHours(10L * 1000L * 60L * 60L).Should().Be(10L);
            TimeUnit.Milliseconds.ToDays(10L * 1000L * 60L * 60L * 24L).Should().Be(10L);

            TimeUnit.Seconds.ToNanoseconds(10L).Should().Be(10L * 1000L * 1000L * 1000L);
            TimeUnit.Seconds.ToMicroseconds(10L).Should().Be(10L * 1000L * 1000L);
            TimeUnit.Seconds.ToMilliseconds(10L).Should().Be(10L * 1000L);
            TimeUnit.Seconds.ToSeconds(10L).Should().Be(10L);
            TimeUnit.Seconds.ToMinutes(10L * 60L).Should().Be(10L);
            TimeUnit.Seconds.ToHours(10L * 60L * 60L).Should().Be(10L);
            TimeUnit.Seconds.ToDays(10L * 60L * 60L * 24L).Should().Be(10L);

            TimeUnit.Minutes.ToNanoseconds(10L).Should().Be(10L * 1000L * 1000L * 1000L * 60L);
            TimeUnit.Minutes.ToMicroseconds(10L).Should().Be(10L * 1000L * 1000L * 60L);
            TimeUnit.Minutes.ToMilliseconds(10L).Should().Be(10L * 1000L * 60L);
            TimeUnit.Minutes.ToSeconds(10L).Should().Be(10L * 60L);
            TimeUnit.Minutes.ToMinutes(10L).Should().Be(10L);
            TimeUnit.Minutes.ToHours(10L * 60L).Should().Be(10L);
            TimeUnit.Minutes.ToDays(10L * 60L * 24L).Should().Be(10L);

            TimeUnit.Hours.ToNanoseconds(10L).Should().Be(10L * 1000L * 1000L * 1000L * 60L * 60L);
            TimeUnit.Hours.ToMicroseconds(10L).Should().Be(10L * 1000L * 1000L * 60L * 60L);
            TimeUnit.Hours.ToMilliseconds(10L).Should().Be(10L * 1000L * 60L * 60L);
            TimeUnit.Hours.ToSeconds(10L).Should().Be(10L * 60L * 60L);
            TimeUnit.Hours.ToMinutes(10L).Should().Be(10L * 60L);
            TimeUnit.Hours.ToHours(10L).Should().Be(10L);
            TimeUnit.Hours.ToDays(10L * 24L).Should().Be(10L);

            TimeUnit.Days.ToNanoseconds(10L).Should().Be(10L * 1000L * 1000L * 1000L * 60L * 60L * 24L);
            TimeUnit.Days.ToMicroseconds(10L).Should().Be(10L * 1000L * 1000L * 60L * 60L * 24L);
            TimeUnit.Days.ToMilliseconds(10L).Should().Be(10L * 1000L * 60L * 60L * 24L);
            TimeUnit.Days.ToSeconds(10L).Should().Be(10L * 60L * 60L * 24L);
            TimeUnit.Days.ToMinutes(10L).Should().Be(10L * 60L * 24L);
            TimeUnit.Days.ToHours(10L).Should().Be(10L * 24L);
            TimeUnit.Days.ToDays(10L).Should().Be(10L);
        }
    }
}
