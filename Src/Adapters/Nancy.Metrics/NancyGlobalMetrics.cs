
using System;
using Metrics;
using Metrics.Core;
using Metrics.Utils;
using Nancy.Bootstrapper;
namespace Nancy.Metrics
{
    public class NancyGlobalMetrics : IHideObjectMembers
    {
        private const string TimerItemsKey = "__Mertics.RequestTimer__";
        private const string RequestStartTimeKey = "__Metrics.RequestStartTime__";

        private readonly MetricsRegistry registry;

        public NancyGlobalMetrics(MetricsRegistry repository)
        {
            this.registry = repository;
            this.MetricsPrefix = "NancyFx";
        }

        public string MetricsPrefix { get; set; }

        /// <summary>
        /// Registers a Meter metric named "NancyFx.Errors" that records the rate at witch unhanded errors occurred while 
        /// processing Nancy requests.
        /// Registers a Timer metric named "NancyFx.Requests" that records how many requests per second are handled and also
        /// keeps a histogram of the request duration.
        /// Registers a counter for the number of active requests.
        /// Registers a histogram for the size of the POST and PUT requests.
        /// Registers a timer metric for each non-error request.
        /// </summary>
        /// <param name="nancyPipelines">Pipelines to hook on.</param>
        public void RegisterAllMetrics(IPipelines nancyPipelines)
        {
            RegisterRequestTimer(nancyPipelines);
            RegisterErrorsMeter(nancyPipelines);
            RegisterActiveRequestCounter(nancyPipelines);
            RegisterPostAndPutRequestSizeHistogram(nancyPipelines);
            RegisterTimerForEachRequest(nancyPipelines);
        }

        /// <summary>
        /// Registers a Timer metric named "NancyFx.Requests" that records how many requests per second are handled and also
        /// keeps a histogram of the request duration.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        /// <param name="nancyPipelines">Pipelines to hook on.</param>
        public void RegisterRequestTimer(IPipelines nancyPipelines, string metricName = "Requests")
        {
            var requestTimer = this.registry.Timer(Name(metricName), Unit.Requests, SamplingType.FavourRecent, TimeUnit.Seconds, TimeUnit.Milliseconds);

            nancyPipelines.BeforeRequest.AddItemToStartOfPipeline(ctx =>
            {
                ctx.Items[TimerItemsKey] = requestTimer.NewContext();
                return null;
            });

            nancyPipelines.AfterRequest.AddItemToEndOfPipeline(ctx =>
            {
                object timer;
                if (ctx.Items.TryGetValue(TimerItemsKey, out timer))
                {
                    using (timer as IDisposable) { }
                    ctx.Items.Remove(TimerItemsKey);
                }
            });
        }

        /// <summary>
        /// Registers a Meter metric named "NancyFx.Errors" that records the rate at witch unhanded errors occurred while 
        /// processing Nancy requests.
        /// </summary>
        /// <param name="nancyPipelines">Pipelines to hook on.</param>
        /// <param name="metricName">Name of the metric.</param>
        public void RegisterErrorsMeter(IPipelines nancyPipelines, string metricName = "Errors")
        {
            var errorMeter = this.registry.Meter(Name(metricName), Unit.Errors, TimeUnit.Seconds);

            nancyPipelines.OnError.AddItemToStartOfPipeline((ctx, ex) =>
            {
                errorMeter.Mark();
                return null;
            });
        }

        /// <summary>
        /// Registers a Counter metric named "NancyFx.ActiveRequests" that shows the current number of active requests
        /// </summary>
        /// <param name="nancyPipelines">Pipelines to hook on.</param>
        /// <param name="metricName">Name of the metric.</param>
        public void RegisterActiveRequestCounter(IPipelines nancyPipelines, string metricName = "ActiveRequests")
        {
            var counter = this.registry.Counter(Name(metricName), Unit.Custom("ActiveRequests"));

            nancyPipelines.BeforeRequest.AddItemToStartOfPipeline(ctx =>
            {
                counter.Increment();
                return null;
            });

            nancyPipelines.AfterRequest.AddItemToEndOfPipeline(ctx =>
            {
                counter.Decrement();
            });
        }

        /// <summary>
        /// Register a Histogram metric named "Nancy.PostAndPutRequestsSize" on the size of the POST and PUT requests
        /// </summary>
        /// <param name="nancyPipelines">Pipelines to hook on.</param>
        /// <param name="metricName">Name of the metric.</param>
        public void RegisterPostAndPutRequestSizeHistogram(IPipelines nancyPipelines, string metricName = "PostAndPutRequestsSize")
        {
            var histogram = this.registry.Histogram(Name(metricName), Unit.Bytes, SamplingType.FavourRecent);

            nancyPipelines.BeforeRequest.AddItemToStartOfPipeline(ctx =>
            {
                var method = ctx.Request.Method.ToUpper();
                if (method == "POST" || method == "PUT")
                {
                    histogram.Update(ctx.Request.Headers.ContentLength);
                }
                return null;
            });
        }

        /// <summary>
        /// Registers a timer for each request.
        /// Timer is created based on route and will be named:
        /// NanyFx.{HTTP_METHOD_NAME} [{ROUTE_PATH}]
        /// </summary>
        /// <param name="nancyPipelines">Pipelines to hook on.</param>
        public void RegisterTimerForEachRequest(IPipelines nancyPipelines)
        {
            nancyPipelines.BeforeRequest.AddItemToStartOfPipeline(ctx =>
            {
                ctx.Items["RequestStartTimeKey"] = Clock.Default.Nanoseconds;
                return null;
            });

            nancyPipelines.AfterRequest.AddItemToEndOfPipeline(ctx =>
            {
                if (ctx.ResolvedRoute != null && !(ctx.ResolvedRoute is Routing.NotFoundRoute))
                {
                    string name = string.Format("{0}.{1} [{2}]", this.MetricsPrefix, ctx.ResolvedRoute.Description.Method, ctx.ResolvedRoute.Description.Path);
                    var startTime = (long)ctx.Items["RequestStartTimeKey"];
                    var elapsed = Clock.Default.Nanoseconds - startTime;
                    Metric.Timer(name, Unit.Requests).Record(elapsed, TimeUnit.Nanoseconds);
                }
            });
        }

        private string Name(string name)
        {
            if (!string.IsNullOrEmpty(this.MetricsPrefix))
            {
                return this.MetricsPrefix + "." + name;
            }
            return name;
        }
    }
}
