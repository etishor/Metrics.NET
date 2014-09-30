using System.Net;
using System.Net.Http;
using FluentAssertions;
using Metrics.Utils;
using Microsoft.Owin.Testing;
using Owin;
using Owin.Metrics;
using Xunit;

namespace Metrics.Tests.OwinAdapter
{
    public class OwinMiddlewareTests
    {
        private const int timePerRequest = 100;

        private readonly TestContext context = new TestContext();
        private readonly MetricsConfig config;
        private readonly TestServer server;

        public OwinMiddlewareTests()
        {
            this.config = new MetricsConfig(this.context);

            this.server = TestServer.Create(app =>
            {
                this.config.WithOwin(m => app.Use(m));

                app.Run(ctx =>
                {
                    this.context.Clock.Advance(TimeUnit.Milliseconds, timePerRequest);
                    if (ctx.Request.Path.ToString() == "/test/action")
                    {
                        return ctx.Response.WriteAsync("response");
                    }

                    if (ctx.Request.Path.ToString() == "/test/error")
                    {
                        ctx.Response.StatusCode = 500;
                        return ctx.Response.WriteAsync("response");
                    }

                    if (ctx.Request.Path.ToString() == "/test/size")
                    {
                        return ctx.Response.WriteAsync("response");
                    }

                    if (ctx.Request.Path.ToString() == "/test/post")
                    {
                        return ctx.Response.WriteAsync("response");
                    }

                    ctx.Response.StatusCode = 404;
                    return ctx.Response.WriteAsync("not found");
                });

            });
        }

        [Fact]
        public void OwinMetrics_ShouldBeAbleToRecordErrors()
        {
            context.MeterValue("Owin", "Errors").Count.Should().Be(0);
            server.HttpClient.GetAsync("http://local.test/test/error").Result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            context.MeterValue("Owin", "Errors").Count.Should().Be(1);

            server.HttpClient.GetAsync("http://local.test/test/error").Result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            context.MeterValue("Owin", "Errors").Count.Should().Be(2);
        }

        [Fact]
        public void OwinMetrics_ShouldBeAbleToRecordActiveRequestCounts()
        {
            context.TimerValue("Owin", "Requests").Rate.Count.Should().Be(0);
            server.HttpClient.GetAsync("http://local.test/test/action").Result.StatusCode.Should().Be(HttpStatusCode.OK);
            context.TimerValue("Owin", "Requests").Rate.Count.Should().Be(1);
            server.HttpClient.GetAsync("http://local.test/test/action").Result.StatusCode.Should().Be(HttpStatusCode.OK);
            context.TimerValue("Owin", "Requests").Rate.Count.Should().Be(2);
            server.HttpClient.GetAsync("http://local.test/test/action").Result.StatusCode.Should().Be(HttpStatusCode.OK);
            context.TimerValue("Owin", "Requests").Rate.Count.Should().Be(3);
            server.HttpClient.GetAsync("http://local.test/test/action").Result.StatusCode.Should().Be(HttpStatusCode.OK);
            context.TimerValue("Owin", "Requests").Rate.Count.Should().Be(4);

            var timer = context.TimerValue("Owin", "Requests");

            timer.Histogram.Min.Should().Be(TimeUnit.Milliseconds.ToNanoseconds(timePerRequest));
            timer.Histogram.Max.Should().Be(TimeUnit.Milliseconds.ToNanoseconds(timePerRequest));
            timer.Histogram.Mean.Should().Be(TimeUnit.Milliseconds.ToNanoseconds(timePerRequest));
        }

        [Fact]
        public void OwinMetrics_ShouldRecordHistogramMetricsForPostSizeAndTimePerRequest()
        {
            const string json = "{ 'id': '1'} ";
            var postContent = new StringContent(json);
            postContent.Headers.Add("Content-Length", json.Length.ToString());
            server.HttpClient.PostAsync("http://local.test/test/post", postContent).Wait();

            var histogram = context.HistogramValue("Owin", "Post & Put Request Size");

            histogram.Count.Should().Be(1);
            histogram.LastValue.Should().Be(json.Length);
        }
    }
}
