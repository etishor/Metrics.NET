using System;
using Metrics;
using Nancy.Bootstrapper;

namespace Nancy.Metrics
{
    /// <summary>
    /// Helper class to register a few NancyFx related metrics.
    /// </summary>
    public static class NancyMetrics
    {
        private const string TimerItemsKey = "__Mertics.RequestTimer__";

        internal static string MetricsModulePath = "";
        internal static Action<MetricsModule> MetricsModuleConfig = m => { };

        /// <summary>
        /// Expose the metrics information at:
        /// /metrics in human readable format
        /// /metrics/json in json format
        /// </summary>
        public static void ExposeMetrics() { ExposeMetrics("/metrics", (m) => { }); }

        /// <summary>
        /// Expose the metrics information at:
        /// /{path} in human readable format
        /// /{path}/json in json format
        /// </summary>
        /// <param name="path">path at witch to expose metrics</param>
        public static void ExposeMetrics(string path) { ExposeMetrics(path, (m) => { }); }

        /// <summary>
        /// Expose the metrics information at:
        /// /{path} in human readable format
        /// /{path}/json in json format
        /// </summary>
        /// <param name="path">path at witch to expose metrics</param>
        /// <param name="configuration">Apply additional configuration on module ( like authorization )</param>
        public static void ExposeMetrics(string path, Action<MetricsModule> configuration)
        {
            MetricsModulePath = path;
            MetricsModuleConfig = configuration;
        }

        /// <summary>
        /// Registers a Meter metric named "NancyFx.Errors" that records the rate at witch unhanded errors occurred while 
        /// processing Nancy requests.
        /// Registers a Timer metric named "NancyFx.Requests" that records how many requests per second are handled and also
        /// keeps a histogram of the request duration.
        /// </summary>
        /// <param name="nancyPipelines">Pipelines to hook on.</param>
        /// <param name="metricsPrefix">Optional prefix for the metric name</param>
        public static void RegisterAllMetrics(IPipelines nancyPipelines, string metricsPrefix = "NancyFx")
        {
            RegisterRequestTimer(nancyPipelines, metricsPrefix);
            RegisterErrorsMeter(nancyPipelines, metricsPrefix);
            RegisterActiveRequestCounter(nancyPipelines, metricsPrefix);
        }

        /// <summary>
        /// Registers a Meter metric named "NancyFx.Errors" that records the rate at witch unhanded errors occurred while 
        /// processing Nancy requests.
        /// </summary>
        /// <param name="nancyPipelines">Pipelines to hook on.</param>
        /// <param name="metricName">Name of the metric.</param>
        /// <param name="metricsPrefix">Optional prefix for the metric name</param>
        public static void RegisterErrorsMeter(IPipelines nancyPipelines, string metricName = "Errors", string metricsPrefix = "NancyFx")
        {
            var errorMeter = Metric.Meter(Name(metricsPrefix, metricName), Unit.Errors);
            nancyPipelines.OnError.AddItemToStartOfPipeline((ctx, ex) => { errorMeter.Mark(); return null; });
        }

        /// <summary>
        /// Registers a Timer metric named "NancyFx.Requests" that records how many requests per second are handled and also
        /// keeps a histogram of the request duration.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        /// <param name="nancyPipelines">Pipelines to hook on.</param>
        /// <param name="metricsPrefix">Optional prefix for the metric name</param>
        public static void RegisterRequestTimer(IPipelines nancyPipelines, string metricName = "Requests", string metricsPrefix = "NancyFx")
        {
            var requestTimer = Metric.Timer(Name(metricsPrefix, metricName), SamplingType.FavourRecent, Unit.Requests);

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
        /// Registers a Counter metric named "NancyFx.ActiveRequests" that shows the current number of active requests
        /// </summary>
        /// <param name="nancyPipelines">Pipelines to hook on.</param>
        /// <param name="metricName">Name of the metric.</param>
        /// <param name="metricsPrefix">Optional prefix for the metric name</param>
        public static void RegisterActiveRequestCounter(IPipelines nancyPipelines, string metricName = "ActiveRequests", string metricsPrefix = "NancyFx")
        {
            var counter = Metric.Counter(Name(metricsPrefix, metricName), Unit.Custom("ActiveRequests"));

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

        private static string Name(string metricsPrefix, string name)
        {
            if (!string.IsNullOrEmpty(metricsPrefix))
            {
                return metricsPrefix + "." + name;
            }
            return name;
        }
    }
}
