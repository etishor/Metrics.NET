
using FluentAssertions;
using Nancy;
using Nancy.Metrics;
using Nancy.Testing;
using Xunit;

namespace Metrics.Tests.NancyAdapter
{
    public class NancyAdapterTests
    {
        public class TestModule : NancyModule
        {
            public TestModule()
                : base("test")
            {
                Get["/"] = _ => Response.AsText("test");
                Post["/"] = _ => HttpStatusCode.OK;
            }
        }

        [Fact]
        public void NancyMetricsShouldBeAbleToRecordPostRequestSize()
        {
            var browser = new Browser(with =>
            {
                with.ApplicationStartup((c, p) => NancyMetrics.RegisterPostRequestSizeHistogram(p, "test", "nancy"));
                with.Module<TestModule>();
            });

            var response = browser.Post("/test", ctx => ctx.Body("test"));

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var histogram = Metric.Histogram("nancy.test", Unit.None).Value;
            histogram.Count.Should().Be(1);
            histogram.Max.Should().Be("test".Length);
        }

        [Fact]
        public void NancyMetricsShouldBeAbleToRecordGetResponseSize()
        {
            var browser = new Browser(with =>
            {
                with.ApplicationStartup((c, p) => NancyMetrics.RegisterGetResponseSizeHistogram(p, "test", "nancy"));
                with.Module<TestModule>();
            });

            var response = browser.Get("/test");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var histogram = Metric.Histogram("nancy.test", Unit.None).Value;
            histogram.Count.Should().Be(1);
            histogram.Max.Should().Be("test".Length);
        }
    }
}
