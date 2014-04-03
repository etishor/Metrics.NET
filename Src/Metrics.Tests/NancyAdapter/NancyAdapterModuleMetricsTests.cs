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
                this.MetricForRequestTimeAndResponseSize("ActionRequest", "Get", "/");

                Get["/action"] = _ =>
                {
                    clock.Advance(TimeUnit.Milliseconds, 100);
                    return Response.AsText("response");
                };

                Get["/contentWithLength"] = _ =>
                {
                    clock.Advance(TimeUnit.Milliseconds, 100);
                    return Response.AsText("response").WithHeader("Content-Length", "100");
                };
            }
        }

        private readonly Clock.TestClock clock;
        private readonly Timer timer;
        private readonly Histogram sizeHistogram;
        private readonly Browser browser;

        public NancyAdapterModuleMetricsTests()
        {
            this.clock = new Clock.TestClock();
            this.timer = new TimerMetric(SamplingType.SlidingWindow, clock);
            this.sizeHistogram = new HistogramMetric();
            NancyMetrics.Configure(new TestRegistry { TimerInstance = timer, HistogramInstance = sizeHistogram });

            this.browser = new Browser(with =>
            {
                with.Module(new TestModule(this.clock));
            });
        }


        [Fact]
        public void NancyMetricsShouldBeAbleToMonitorTimeForModuleRequest()
        {
            browser.Get("/test/action").StatusCode.Should().Be(HttpStatusCode.OK);

            timer.Value.Rate.Count.Should().Be(1);
            timer.Value.Histogram.Count.Should().Be(1);
            timer.Value.Histogram.Max.Should().Be(TimeUnit.Milliseconds.ToNanoseconds(100));
        }

        [Fact]
        public void NancyMetricsShouldBeAbleToMonitorSizeForModuleRequest()
        {
            browser.Get("/test/action").StatusCode.Should().Be(HttpStatusCode.OK);

            sizeHistogram.Value.Count.Should().Be(1);
            sizeHistogram.Value.Min.Should().Be("response".Length);
            sizeHistogram.Value.Max.Should().Be("response".Length);

            browser.Get("/test/contentWithLength").StatusCode.Should().Be(HttpStatusCode.OK);

            sizeHistogram.Value.Count.Should().Be(2);
            sizeHistogram.Value.Min.Should().Be("response".Length);
            sizeHistogram.Value.Max.Should().Be(100);
        }
    }
}
