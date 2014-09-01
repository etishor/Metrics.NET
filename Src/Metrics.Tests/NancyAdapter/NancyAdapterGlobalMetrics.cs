
using System;
using System.Threading.Tasks;
using FluentAssertions;
using Metrics.Core;
using Metrics.Tests.TestUtils;
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

        private readonly TestClock clock;
        private readonly TestScheduler scheduler;
        private readonly TimerMetric timer;
        private readonly MeterMetric meter;
        private readonly CounterMetric counter;
        private readonly HistogramMetric size;
        private readonly Browser browser;

        private readonly TaskCompletionSource<int> requestTrigger = new TaskCompletionSource<int>();
        private readonly TaskCompletionSource<int> result1 = new TaskCompletionSource<int>();
        private readonly TaskCompletionSource<int> result2 = new TaskCompletionSource<int>();

        public NancyAdapterGlobalMetrics()
        {
            this.clock = new TestClock();
            this.scheduler = new TestScheduler(clock);

            this.timer = new TimerMetric(SamplingType.SlidingWindow, new MeterMetric(clock, scheduler), clock);
            this.meter = new MeterMetric(clock, scheduler);
            this.counter = new CounterMetric();
            this.size = new HistogramMetric();

            this.browser = new Browser(with =>
            {
                with.ApplicationStartup((c, p) =>
                {
                    var registry = new TestRegistry
                    {
                        TimerInstance = timer,
                        MeterInstance = meter,
                        CounterInstance = counter,
                        HistogramInstance = size
                    };
                    var context = new MetricContext("test", registry);

                    context.Config.WithNancy(nancy => nancy.WithNancyMetrics(config => config.RegisterAllMetrics(p)), contextName: string.Empty);
                });
                with.Module(new TestModule(this.clock));
                with.Module(new ActiveRequestsModule(this.requestTrigger.Task, result1, result2));
            });
        }

        [Fact]
        public void NancyMetricsShouldBeAbleToRecordTimeForAllRequests()
        {
            timer.Value.Rate.Count.Should().Be(0);

            browser.Get("/test/action").StatusCode.Should().Be(HttpStatusCode.OK);

            timer.Value.Rate.Count.Should().Be(2); // from all requests timer and from each request timer
            timer.Value.Histogram.Count.Should().Be(2);// from all requests timer and from each request timer
            //timer.Value.Histogram.Max.Should().Be(TimeUnit.Milliseconds.ToNanoseconds(100));
            //timer.Value.Histogram.Min.Should().Be(TimeUnit.Milliseconds.ToNanoseconds(100));

            browser.Post("/test/post").StatusCode.Should().Be(HttpStatusCode.OK);

            timer.Value.Rate.Count.Should().Be(4);
            timer.Value.Histogram.Count.Should().Be(4);
            //timer.Value.Histogram.Max.Should().Be(TimeUnit.Milliseconds.ToNanoseconds(200));
            //timer.Value.Histogram.Min.Should().Be(TimeUnit.Milliseconds.ToNanoseconds(100));
        }

        [Fact]
        public void NancyMetricsShouldBeAbleToCountErrors()
        {
            meter.Value.Count.Should().Be(0);
            Assert.Throws<Exception>(() => browser.Get("/test/error"));
            meter.Value.Count.Should().Be(1);
            Assert.Throws<Exception>(() => browser.Get("/test/error"));
            meter.Value.Count.Should().Be(2);
        }

        [Fact]
        public void NancyMetricsShouldBeAbleToCountActiveRequests()
        {
            counter.Value.Should().Be(0);
            var request1 = Task.Factory.StartNew(() => browser.Get("/concurrent/request1"));

            result1.Task.Wait();
            counter.Value.Should().Be(1);

            var request2 = Task.Factory.StartNew(() => browser.Get("/concurrent/request2"));
            result2.Task.Wait();
            counter.Value.Should().Be(2);

            requestTrigger.SetResult(0);
            Task.WaitAll(request1, request2);
            counter.Value.Should().Be(0);
        }

        [Fact]
        public void NancyMetricsShoulBeAbleToRecordPostAndPutRequestSize()
        {
            size.Value.Count.Should().Be(0);

            browser.Get("/test/action").StatusCode.Should().Be(HttpStatusCode.OK);

            size.Value.Count.Should().Be(0);

            browser.Post("/test/post", ctx =>
            {
                ctx.Header("Content-Length", "content".Length.ToString());
                ctx.Body("content");
            }).StatusCode.Should().Be(HttpStatusCode.OK);

            size.Value.Count.Should().Be(1);
            size.Value.Min.Should().Be("content".Length);
        }
    }
}
