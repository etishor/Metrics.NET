using Metrics.Core;
using Owin.Metrics.Middleware;

namespace Owin.Metrics
{
    public class OwinMetricsConfig
    {
        private readonly MetricsRegistry _metricsRegistry;
        private IAppBuilder _app;

        public OwinMetricsConfig(IAppBuilder app, MetricsRegistry metricsRegistry)
        {
            _app = app;
            _metricsRegistry = metricsRegistry;
        }

        public MetricsRegistry Registry { get { return _metricsRegistry; } }

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
            var metricsMiddleware = new RequestTimerMiddleware(_metricsRegistry);
            _app.Use(metricsMiddleware);
            return this;
        }

        public OwinMetricsConfig RegisterActiveRequestCounter()
        {
            var metricsMiddleware = new ActiveRequestCounterMiddleware(_metricsRegistry);
            _app.Use(metricsMiddleware);
            return this;
        }

        public OwinMetricsConfig RegisterPostAndPutRequestSizeHistogram()
        {
            var metricsMiddleware = new PostAndPutRequestSizeHistogramMiddleware(_metricsRegistry);
            _app.Use(metricsMiddleware);
            return this;
        }

        public OwinMetricsConfig RegisterTimerForEachRequest()
        {
            var metricsMiddleware = new TimerForEachRequestMiddleware(_metricsRegistry);
            _app.Use(metricsMiddleware);
            return this;
        }

        public OwinMetricsConfig RegisterErrorsMeter()
        {
            var metricsMiddleware = new ErrorMeterMiddleware(_metricsRegistry);
            _app.Use(metricsMiddleware);
            return this;
        }
    }
}