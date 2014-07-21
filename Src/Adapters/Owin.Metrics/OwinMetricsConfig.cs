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
        }

        public MetricsRegistry Registry { get { return metricsRegistry; } }

        /// <summary>
        /// Configure global WebApi Metrics.
        /// Available global metrics are: Request Timer, Active Requests Counter, Error Meter
        /// </summary>
        /// <returns>
        /// This instance to allow chaining of the configuration.
        /// </returns>
        public OwinMetricsConfig RegisterAllMetrics()
        {
            RegisterRequestTimer();
            RegisterActiveRequestCounter();
            RegisterPostAndPutRequestSizeHistogram();
            RegisterTimerForEachRequest();
            RegisterErrorsMeter();
            return this;
        }

        public OwinMetricsConfig RegisterRequestTimer()
        {
            var metricsMiddleware = new RequestTimerMiddleware(metricsRegistry);
            app.Use(metricsMiddleware);
            return this;
        }

        public OwinMetricsConfig RegisterActiveRequestCounter()
        {
            var metricsMiddleware = new ActiveRequestCounterMiddleware(metricsRegistry);
            app.Use(metricsMiddleware);
            return this;
        }

        public OwinMetricsConfig RegisterPostAndPutRequestSizeHistogram()
        {
            var metricsMiddleware = new PostAndPutRequestSizeHistogramMiddleware(metricsRegistry);
            app.Use(metricsMiddleware);
            return this;
        }

        public OwinMetricsConfig RegisterTimerForEachRequest()
        {
            var metricsMiddleware = new TimerForEachRequestMiddleware(metricsRegistry);
            app.Use(metricsMiddleware);
            return this;
        }

        public OwinMetricsConfig RegisterErrorsMeter()
        {
            var metricsMiddleware = new ErrorMeterMiddleware(metricsRegistry);
            app.Use(metricsMiddleware);
            return this;
        }
    }
}