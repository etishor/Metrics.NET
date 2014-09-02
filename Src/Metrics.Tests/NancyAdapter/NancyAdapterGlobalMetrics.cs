using System;
using System.Threading.Tasks;
using FluentAssertions;
using Metrics.Tests.TestUtils;
using Metrics.Utils;
using Nancy;
using Nancy.Testing;
using Xunit;

namespace Metrics.Tests.NancyAdapter
{
    public class NancyAdapterGlobalMetrics
    {

        public class ActiveRequestsModule : NancyModule
        {
            public ActiveRequestsModule(Task trigger, TaskCompletionSource<int> request1, TaskCompletionSource<int> request2)
                : base("/concurrent")
            {
                Get["/request1"] = _ => { request1.SetResult(0); Task.WaitAll(trigger); return HttpStatusCode.OK; };
                Get["/request2"] = _ => { request2.SetResult(0); Task.WaitAll(trigger); return HttpStatusCode.OK; };
            }
        }

        public class TestModule : NancyModule
        {
            public TestModule(TestClock clock)
                : base("/test")
            {
                Get["/action"] = _ =>
                {
                    clock.Advance(TimeUnit.Milliseconds, 100);
                    return Response.AsText("response");
                };

                Post["/post"] = _ =>
                {
                    clock.Advance(TimeUnit.Milliseconds, 200);
                    return HttpStatusCode.OK;
                };

                Get["/error"] = _ => { throw new InvalidOperationException(); };
            }
        }

        private readonly TestContext context = new TestContext();

        private readonly Browser browser;

        private readonly TaskCompletionSource<int> requestTrigger = new TaskCompletionSource<int>();
        private readonly TaskCompletionSource<int> result1 = new TaskCompletionSource<int>();
        private readonly TaskCompletionSource<int> result2 = new TaskCompletionSource<int>();

        public NancyAdapterGlobalMetrics()
        {
            this.browser = new Browser(with =>
            {
                with.ApplicationStartup((c, p) =>
                {
                    this.context.Config.WithNancy(nancy => nancy.WithNancyMetrics(config => config.RegisterAllMetrics(p)));
                });

                with.Module(new TestModule(this.context.Clock));
                with.Module(new ActiveRequestsModule(this.requestTrigger.Task, result1, result2));
            });
        }

        [Fact]
        public void NancyMetricsShouldBeAbleToRecordTimeForAllRequests()
        {
            this.context.ForAllTimers(v => v.Rate.Count.Should().Be(0));

            browser.Get("/test/action").StatusCode.Should().Be(HttpStatusCode.OK);

            var timer = this.context.TimerValue("NancyFx.Requests");

            timer.Rate.Count.Should().Be(1);
            timer.Histogram.Count.Should().Be(1);

            timer.Histogram.Max.Should().Be(TimeUnit.Milliseconds.ToNanoseconds(100));
            timer.Histogram.Min.Should().Be(TimeUnit.Milliseconds.ToNanoseconds(100));

            browser.Post("/test/post").StatusCode.Should().Be(HttpStatusCode.OK);

            timer = this.context.TimerValue("NancyFx.Requests");

            timer.Rate.Count.Should().Be(2);
            timer.Histogram.Count.Should().Be(2);

            timer.Histogram.Max.Should().Be(TimeUnit.Milliseconds.ToNanoseconds(200));
            timer.Histogram.Min.Should().Be(TimeUnit.Milliseconds.ToNanoseconds(100));
        }

        //[Fact]
        //public void NancyMetricsShouldBeAbleToCountErrors()
        //{
        //    meter.Value.Count.Should().Be(0);
        //    Assert.Throws<Exception>(() => browser.Get("/test/error"));
        //    meter.Value.Count.Should().Be(1);
        //    Assert.Throws<Exception>(() => browser.Get("/test/error"));
        //    meter.Value.Count.Should().Be(2);
        //}

        //[Fact]
        //public void NancyMetricsShouldBeAbleToCountActiveRequests()
        //{
        //    counter.Value.Should().Be(0);
        //    var request1 = Task.Factory.StartNew(() => browser.Get("/concurrent/request1"));

        //    result1.Task.Wait();
        //    counter.Value.Should().Be(1);

        //    var request2 = Task.Factory.StartNew(() => browser.Get("/concurrent/request2"));
        //    result2.Task.Wait();
        //    counter.Value.Should().Be(2);

        //    requestTrigger.SetResult(0);
        //    Task.WaitAll(request1, request2);
        //    counter.Value.Should().Be(0);
        //}

        //[Fact]
        //public void NancyMetricsShoulBeAbleToRecordPostAndPutRequestSize()
        //{
        //    size.Value.Count.Should().Be(0);

        //    browser.Get("/test/action").StatusCode.Should().Be(HttpStatusCode.OK);

        //    size.Value.Count.Should().Be(0);

        //    browser.Post("/test/post", ctx =>
        //    {
        //        ctx.Header("Content-Length", "content".Length.ToString());
        //        ctx.Body("content");
        //    }).StatusCode.Should().Be(HttpStatusCode.OK);

        //    size.Value.Count.Should().Be(1);
        //    size.Value.Min.Should().Be("content".Length);
        //}
    }
}
