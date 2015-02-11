using System.Globalization;
using System.Threading;
using FluentAssertions;
using Metrics.Json;
using Xunit;

namespace Metrics.Tests.Json
{
    public class JsonTests
    {
        [Fact]
        public void Json_DoubleJsonValueSerializesCultureInvariant()
        {
            double value = 0.5d;

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

            value.ToString("F").Should().Be("0,50");
            new DoubleJsonValue(value).AsJson().Should().Be("0.50");

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

            value.ToString("F").Should().Be("0,50");
            new DoubleJsonValue(value).AsJson().Should().Be("0.50");

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

            value.ToString("F").Should().Be("0.50");
            new DoubleJsonValue(value).AsJson().Should().Be("0.50");
        }

        [Fact]
        public void Json_LongJsonValueSerializesCultureInvariant()
        {
            long value = 1000 * 1000 * 1000;

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

            value.ToString("D").Should().Be("1000000000");
            new LongJsonValue(value).AsJson().Should().Be("1000000000");

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

            value.ToString("D").Should().Be("1000000000");
            new LongJsonValue(value).AsJson().Should().Be("1000000000");

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

            value.ToString("D").Should().Be("1000000000");
            new LongJsonValue(value).AsJson().Should().Be("1000000000");
        }

        [Fact]
        public void Json_DoubleNanIsSerialziedCorrectly()
        {
            new DoubleJsonValue(double.NaN).AsJson().Should().Be("null");
        }
    }
}
