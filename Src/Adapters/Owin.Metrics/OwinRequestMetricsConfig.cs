using System;
using System.Text.RegularExpressions;
using Metrics;
using Owin.Metrics.Middleware;

namespace Owin.Metrics
{
    public class OwinRequestMetricsConfig
    {
        private readonly MetricsContext metricsContext;
        private readonly Action<object> middlewareRegistration;
        private readonly Regex[] ignoreRequestPathPatterns;

        public OwinRequestMetricsConfig(Action<object> middlewareRegistration, MetricsContext metricsContext, Regex[] ignoreRequestPathPatterns)
        {
            this.middlewareRegistration = middlewareRegistration;
            this.metricsContext = metricsContext;
            this.ignoreRequestPathPatterns = ignoreRequestPathPatterns;
        }

        /// <summary>
        /// Configure global OWIN Metrics.
        /// Available global metrics are: Request Timer, Active Requests Counter, Error Meter
        /// </summary>
        /// <returns>
        /// This instance to allow chaining of the configuration.
        /// </returns>
        public OwinRequestMetricsConfig WithAllOwinMetrics()
        {
            WithRequestTimer();
            WithActiveRequestCounter();
            WithPostAndPutRequestSizeHistogram();
            WithTimerForEachRequest();
            WithErrorsMeter();
            return this;
        }

        /// <summary>
        /// Registers a Timer metric named "Owin.Requests" that records how many requests per second are handled and also
        /// keeps a histogram of the request duration.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        public OwinRequestMetricsConfig WithRequestTimer(string metricName = "Requests")
        {
            var metricsMiddleware = new RequestTimerMiddleware(this.metricsContext, metricName, this.ignoreRequestPathPatterns);
            middlewareRegistration(metricsMiddleware);
            return this;
        }

        /// <summary>
        /// Registers a Counter metric named "Owin.ActiveRequests" that shows the current number of active requests
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        public OwinRequestMetricsConfig WithActiveRequestCounter(string metricName = "Active Requests")
        {
            var metricsMiddleware = new ActiveRequestCounterMiddleware(this.metricsContext, metricName, this.ignoreRequestPathPatterns);
            middlewareRegistration(metricsMiddleware);
            return this;
        }

        /// <summary>
        /// Register a Histogram metric named "Owin.PostAndPutRequestsSize" on the size of the POST and PUT requests
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        public OwinRequestMetricsConfig WithPostAndPutRequestSizeHistogram(string metricName = "Post & Put Request Size")
        {
            var metricsMiddleware = new PostAndPutRequestSizeHistogramMiddleware(this.metricsContext, metricName, this.ignoreRequestPathPatterns);
            middlewareRegistration(metricsMiddleware);
            return this;
        }

        /// <summary>
        /// Registers a timer for each request.
        /// Timer is created based on route and will be named:
        /// Owin.{HTTP_METHOD_NAME} [{ROUTE_PATH}]
        /// </summary>
        public OwinRequestMetricsConfig WithTimerForEachRequest()
        {
            var metricsMiddleware = new TimerForEachRequestMiddleware(this.metricsContext, this.ignoreRequestPathPatterns);
            middlewareRegistration(metricsMiddleware);
            return this;
        }

        /// <summary>
        /// Registers a Meter metric named "Owin.Errors" that records the rate at witch unhanded errors occurred while 
        /// processing Nancy requests.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        public OwinRequestMetricsConfig WithErrorsMeter(string metricName = "Errors")
        {
            var metricsMiddleware = new ErrorMeterMiddleware(this.metricsContext, metricName, this.ignoreRequestPathPatterns);
            middlewareRegistration(metricsMiddleware);
            return this;
        }
    }
}