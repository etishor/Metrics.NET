using FluentAssertions;
using Metrics.Core;
using Metrics.Tests.TestUtils;
using Metrics.Utils;
using Nancy;
using Nancy.Metrics;
using Nancy.Testing;
using Xunit;

namespace Metrics.Tests.NancyAdapter
{
    public class NancyAdapterModuleMetricsTests
    {
        public class TestModule : NancyModule
        {
            public TestModule(Clock.TestClock clock)
                : base("/test")
            {
                this.MetricForRequestTime("Action", "Get", "/action");

                Get["/action"] = _ =>
                {
                    clock.Advance(TimeUnit.Milliseconds, 100);
                    return Response.AsText("response");
                };
            }
        }

        [Fact]
        public void NancyMetricsShouldBeAbleMonitorModuleRequest()
        {
            Clock.TestClock clock = new Clock.TestClock();
            Timer timer = new TimerMetric(SamplingType.LongTerm, clock);
            NancyMetrics.Configure(new TestRegistry(timer));

            var browser = new Browser(with =>
            {
                with.Module(new TestModule(clock));
            });

            var response = browser.Get("/test/action");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var value = timer.Value;
            value.Rate.Count.Should().Be(1);
            value.Histogram.Count.Should().Be(1);
            value.Histogram.Max.Should().Be(TimeUnit.Milliseconds.ToNanoseconds(100));
        }
    }
}
