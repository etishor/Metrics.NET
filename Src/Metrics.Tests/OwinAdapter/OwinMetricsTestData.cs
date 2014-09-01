using System.Net;
using System.Net.Http;
using FluentAssertions;
using Metrics.Core;
using Metrics.Tests.TestUtils;
using Microsoft.Owin.Testing;
using Owin;
using Owin.Metrics;

namespace Metrics.Tests.OwinAdapter
{
    public class OwinMetricsTestData
    {
        public TestClock Clock { get; private set; }
        public TimerMetric TimerMetric { get; private set; }
        public TestCounter CounterMetric { get; private set; }
        public HistogramMetric HistogramMetric { get; private set; }
        public MeterMetric MeterMetric { get; private set; }

        public OwinExpectedMetrics ExpectedResults { get; private set; }

        public OwinMetricsTestData()
        {
            const int timePerRequest = 100;
            const string json = "{ 'id': '1'} ";

            Clock = new TestClock();
            var scheduler = new TestScheduler(Clock);
            TimerMetric = new TimerMetric(SamplingType.SlidingWindow, new MeterMetric(Clock, scheduler), Clock);
            CounterMetric = new TestCounter();
            HistogramMetric = new HistogramMetric();
            MeterMetric = new MeterMetric(Clock, scheduler);

            var server = TestServer.Create(app =>
            {
                var registery = new TestRegistry
                {
                    TimerInstance = TimerMetric,
                    CounterInstance = CounterMetric,
                    HistogramInstance = HistogramMetric,
                    MeterInstance = MeterMetric
                };

                var metricsContext = new MetricsContext("test", registery);

                OwinMetricsConfig owin = new OwinMetricsConfig(middleware => app.Use(middleware), metricsContext, () => HealthChecks.GetStatus());
                owin.WithRequestMetricsConfig(c => c.RegisterAllMetrics());

                app.Run(context =>
                {
                    Clock.Advance(TimeUnit.Milliseconds, timePerRequest);
                    if (context.Request.Path.ToString() == "/test/action")
                    {
                        return context.Response.WriteAsync("response");
                    }

                    if (context.Request.Path.ToString() == "/test/error")
                    {
                        context.Response.StatusCode = 500;
                        return context.Response.WriteAsync("response");
                    }

                    if (context.Request.Path.ToString() == "/test/size")
                    {
                        return context.Response.WriteAsync("response");
                    }

                    if (context.Request.Path.ToString() == "/test/post")
                    {
                        return context.Response.WriteAsync("response");
                    }

                    context.Response.StatusCode = 404;
                    return context.Response.WriteAsync("not found");
                });

            });

            ExpectedResults = new OwinExpectedMetrics(timePerRequest, 6, 1);

            server.HttpClient.GetAsync("http://local.test/test/error").Result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
            server.HttpClient.GetAsync("http://local.test/test/action").Result.StatusCode.Should().Be(HttpStatusCode.OK);
            server.HttpClient.GetAsync("http://local.test/test/action").Result.StatusCode.Should().Be(HttpStatusCode.OK);
            server.HttpClient.GetAsync("http://local.test/test/action").Result.StatusCode.Should().Be(HttpStatusCode.OK);
            server.HttpClient.GetAsync("http://local.test/test/action").Result.StatusCode.Should().Be(HttpStatusCode.OK);
            var postContent = new StringContent(json);
            postContent.Headers.Add("Content-Length", json.Length.ToString());
            server.HttpClient.PostAsync("http://local.test/test/post", postContent);
        }
    }
}