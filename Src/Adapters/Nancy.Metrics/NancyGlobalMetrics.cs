
using System;
using Metrics;
using Nancy.Bootstrapper;
namespace Nancy.Metrics
{
    public class NancyGlobalMetrics : IHideObjectMembers
    {
        private const string TimerItemsKey = "__Mertics.RequestTimer__";

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
        /// </summary>
        /// <param name="nancyPipelines">Pipelines to hook on.</param>
        public void RegisterAllMetrics(IPipelines nancyPipelines)
        {
            RegisterRequestTimer(nancyPipelines);
            RegisterErrorsMeter(nancyPipelines);
            RegisterActiveRequestCounter(nancyPipelines);
            RegisterPostAndPutRequestSizeHistogram(nancyPipelines);
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
                using (ctx.Items[TimerItemsKey] as IDisposable) { }
                ctx.Items.Remove(TimerItemsKey);
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
            var histogram = this.registry.Histogram(Name(metricName), Unit.Custom("bytes"), SamplingType.FavourRecent);

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
