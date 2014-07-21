using Metrics.Core;
using Owin.Metrics.Middleware;

namespace Owin.Metrics
{
    public class OwinMetricsConfig
    {
        private readonly MetricsRegistry metricsRegistry;
        private IAppBuilder app;

        public OwinMetricsConfig(IAppBuilder app, MetricsRegistry metricsRegistry)
        {
            this.app = app;
            this.metricsRegistry = metricsRegistry;
            this.MetricsPrefix = "Owin";
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
        public OwinMetricsConfig RegisterAllMetrics(string metricsPrexix = "Owin")
        {
            this.MetricsPrefix = metricsPrexix;
            RegisterRequestTimer();
            RegisterActiveRequestCounter();
            RegisterPostAndPutRequestSizeHistogram();
            RegisterTimerForEachRequest();
            RegisterErrorsMeter();
            return this;
        }

        public OwinMetricsConfig RegisterRequestTimer(string metricName = "Requests")
        {
            var metricsMiddleware = new RequestTimerMiddleware(metricsRegistry, Name(metricName));
            app.Use(metricsMiddleware);
            return this;
        }

        public OwinMetricsConfig RegisterActiveRequestCounter(string metricName = "ActiveRequests")
        {
            var metricsMiddleware = new ActiveRequestCounterMiddleware(metricsRegistry, Name(metricName));
            app.Use(metricsMiddleware);
            return this;
        }

        public OwinMetricsConfig RegisterPostAndPutRequestSizeHistogram(string metricName = "PostAndPutRequestsSize")
        {
            var metricsMiddleware = new PostAndPutRequestSizeHistogramMiddleware(metricsRegistry, Name(metricName));
            app.Use(metricsMiddleware);
            return this;
        }

        public OwinMetricsConfig RegisterTimerForEachRequest(string metricPrefix = "Owin")
        {
            var metricsMiddleware = new TimerForEachRequestMiddleware(metricsRegistry, metricPrefix);
            app.Use(metricsMiddleware);
            return this;
        }

        public OwinMetricsConfig RegisterErrorsMeter(string metricName = "Errors")
        {
            var metricsMiddleware = new ErrorMeterMiddleware(metricsRegistry, Name(metricName));
            app.Use(metricsMiddleware);
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