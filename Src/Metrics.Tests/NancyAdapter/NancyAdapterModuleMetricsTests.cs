using FluentAssertions;
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
                this.MetricForRequestTimeAndResponseSize("Action Request", "Get", "/");
                this.MetricForRequestSize("Request Size", "Put", "/");

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

        private readonly TestContext context = new TestContext();
        private readonly MetricsConfig config;
        private readonly Browser browser;

        public NancyAdapterModuleMetricsTests()
        {
            this.config = new MetricsConfig(this.context);

            this.browser = new Browser(with =>
            {
                with.ApplicationStartup((c, p) =>
                {
                    this.config.WithNancy(p);
                    with.Module(new TestModule(this.context.Clock));
                });
            });
        }

        [Fact]
        public void NancyMetrics_ShouldBeAbleToMonitorTimeForModuleRequest()
        {
            this.context.TimerValue("NancyFx", "Action Request").Rate.Count.Should().Be(0);
            browser.Get("/test/action").StatusCode.Should().Be(HttpStatusCode.OK);

            var timer = this.context.TimerValue("NancyFx", "Action Request");

            timer.Rate.Count.Should().Be(1);
            timer.Histogram.Count.Should().Be(1);
            timer.Histogram.Max.Should().Be(100);
        }

        [Fact]
        public void NancyMetrics_ShouldBeAbleToMonitorSizeForRouteReponse()
        {
            browser.Get("/test/action").StatusCode.Should().Be(HttpStatusCode.OK);

            var sizeHistogram = this.context.HistogramValue("NancyFx", "Action Request");

            sizeHistogram.Count.Should().Be(1);
            sizeHistogram.Min.Should().Be("response".Length);
            sizeHistogram.Max.Should().Be("response".Length);

            browser.Get("/test/contentWithLength").StatusCode.Should().Be(HttpStatusCode.OK);

            sizeHistogram = this.context.HistogramValue("NancyFx", "Action Request");

            sizeHistogram.Count.Should().Be(2);
            sizeHistogram.Min.Should().Be("response".Length);
            sizeHistogram.Max.Should().Be(100);
        }

        [Fact]
        public void NancyMetrics_ShouldBeAbleToMonitorSizeForRequest()
        {
            this.context.HistogramValue("NancyFx", "Request Size").Count.Should().Be(0);

            browser.Put("/test/size", ctx =>
            {
                ctx.Header("Content-Length", "content".Length.ToString());
                ctx.Body("content");
            }).StatusCode.Should().Be(HttpStatusCode.OK);

            var sizeHistogram = this.context.HistogramValue("NancyFx", "Request Size");

            sizeHistogram.Count.Should().Be(1);
            sizeHistogram.Min.Should().Be("content".Length);
        }
    }
}
