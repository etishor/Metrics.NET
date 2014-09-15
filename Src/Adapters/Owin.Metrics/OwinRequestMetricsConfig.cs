using Metrics.Core;
using Owin.Metrics.Middleware;
using System;
using System.Text.RegularExpressions;

namespace Owin.Metrics
{
    public class OwinRequestMetricsConfig
    {
        private readonly MetricsRegistry metricsRegistry;
        private readonly Action<object> middlewareRegistration;
        private Regex[] ignoreRequestPathPatterns;

        public OwinRequestMetricsConfig(Action<object> middlewareRegistration, MetricsRegistry metricsRegistry,
            Regex[] ignoreRequestPathPatterns)
        {
            this.middlewareRegistration = middlewareRegistration;
            this.metricsRegistry = metricsRegistry;
            this.MetricsPrefix = "Owin";
            this.ignoreRequestPathPatterns = ignoreRequestPathPatterns;
        }

        public MetricsRegistry Registry { get { return metricsRegistry; } }
        public string MetricsPrefix { get; set; }

        /// <summary>
        /// Configure global OWIN Metrics.
        /// Available global metrics are: Request Timer, Active Requests Counter, Error Meter
        /// </summary>
        /// <returns>
        /// This instance to allow chaining of the configuration.
        /// </returns>
        public OwinRequestMetricsConfig RegisterAllMetrics(string metricsPrefix = "Owin")
        {
            this.MetricsPrefix = metricsPrefix;
            RegisterRequestTimer();
            RegisterActiveRequestCounter();
            RegisterPostAndPutRequestSizeHistogram();
            RegisterTimerForEachRequest();
            RegisterErrorsMeter();
            return this;
        }

        /// <summary>
        /// Registers a Timer metric named "Owin.Requests" that records how many requests per second are handled and also
        /// keeps a histogram of the request duration.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        public OwinRequestMetricsConfig RegisterRequestTimer(string metricName = "Requests")
        {
            var metricsMiddleware = new RequestTimerMiddleware(metricsRegistry, Name(metricName), this.ignoreRequestPathPatterns);
            middlewareRegistration(metricsMiddleware);
            return this;
        }

        /// <summary>
        /// Registers a Counter metric named "Owin.ActiveRequests" that shows the current number of active requests
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        public OwinRequestMetricsConfig RegisterActiveRequestCounter(string metricName = "ActiveRequests")
        {
            var metricsMiddleware = new ActiveRequestCounterMiddleware(metricsRegistry, Name(metricName), this.ignoreRequestPathPatterns);
            middlewareRegistration(metricsMiddleware);
            return this;
        }

        /// <summary>
        /// Register a Histogram metric named "Owin.PostAndPutRequestsSize" on the size of the POST and PUT requests
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        public OwinRequestMetricsConfig RegisterPostAndPutRequestSizeHistogram(string metricName = "PostAndPutRequestsSize")
        {
            var metricsMiddleware = new PostAndPutRequestSizeHistogramMiddleware(metricsRegistry, Name(metricName), this.ignoreRequestPathPatterns);
            middlewareRegistration(metricsMiddleware);
            return this;
        }

        /// <summary>
        /// Registers a timer for each request.
        /// Timer is created based on route and will be named:
        /// Owin.{HTTP_METHOD_NAME} [{ROUTE_PATH}]
        /// </summary>
        public OwinRequestMetricsConfig RegisterTimerForEachRequest(string metricPrefix = "Owin")
        {
            var metricsMiddleware = new TimerForEachRequestMiddleware(metricsRegistry, metricPrefix, this.ignoreRequestPathPatterns);
            middlewareRegistration(metricsMiddleware);
            return this;
        }

        /// <summary>
        /// Registers a Meter metric named "Owin.Errors" that records the rate at witch unhanded errors occurred while 
        /// processing Nancy requests.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        public OwinRequestMetricsConfig RegisterErrorsMeter(string metricName = "Errors")
        {
            var metricsMiddleware = new ErrorMeterMiddleware(metricsRegistry, Name(metricName), this.ignoreRequestPathPatterns);
            middlewareRegistration(metricsMiddleware);
            return this;
        }

        private string Name(string name)
        {
            if (!string.IsNullOrEmpty(MetricsPrefix))
            {
                return MetricsPrefix + "." + name;
            }
            return name;
        }
    }
}