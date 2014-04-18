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
            public TestModule(TestClock clock)
                : base("/test")
            {
                this.MetricForRequestTimeAndResponseSize("ActionRequest", "Get", "/");
                this.MetricForRequestSize("RequestSize", "Put", "/");

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

                Put["/size"] = _ => HttpStatusCode.OK;
            }
        }

        private readonly TestClock clock;
        private readonly TimerMetric timer;
        private readonly HistogramMetric sizeHistogram;
        private readonly Browser browser;

        public NancyAdapterModuleMetricsTests()
        {
            this.clock = new TestClock();
            TestScheduler scheduler = new TestScheduler(clock);

            this.timer = new TimerMetric(SamplingType.SlidingWindow, new MeterMetric(clock, scheduler), clock);
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
        public void NancyMetricsShouldBeAbleToMonitorSizeForRouteReponse()
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

        [Fact]
        public void NancyMetricsShouldBeAbleToMonitorSizeForRequest()
        {
            sizeHistogram.Value.Count.Should().Be(0);

            browser.Put("/test/size", ctx =>
            {
                ctx.Header("Content-Length", "content".Length.ToString());
                ctx.Body("content");
            }).StatusCode.Should().Be(HttpStatusCode.OK);

            sizeHistogram.Value.Count.Should().Be(1);
            sizeHistogram.Value.Min.Should().Be("content".Length);
        }
    }
}
