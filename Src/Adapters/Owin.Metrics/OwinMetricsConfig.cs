using Metrics;
using Owin.Metrics.Middleware;
using System;
using System.Text.RegularExpressions;
namespace Owin.Metrics
{
    public class OwinMetricsConfig
    {
        private readonly Action<object> middlewareRegistration;
        private readonly MetricsContext context;
        private readonly Func<HealthStatus> healthStatus;

        public OwinMetricsConfig(Action<object> middlewareRegistration, MetricsContext context, Func<HealthStatus> healthStatus)
        {
            this.middlewareRegistration = middlewareRegistration;
            this.context = context;
            this.healthStatus = healthStatus;
        }

        /// <summary>
        /// Register all predefined metrics.
        /// </summary>
        /// <param name="ignoreRequestPathPatterns">Patterns for paths to ignore.</param>
        /// <param name="owinContext">Name of the metrics context where to register the metrics.</param>
        /// <returns>Chainable configuration object.</returns>
        public OwinMetricsConfig WithRequestMetricsConfig(Regex[] ignoreRequestPathPatterns = null, string owinContext = "Owin")
        {
            return WithRequestMetricsConfig(config => config.WithAllOwinMetrics(), ignoreRequestPathPatterns, owinContext);
        }

        /// <summary>
        /// Configure which Owin metrics to be registered.
        /// </summary>
        /// <param name="config">Action used to configure Owin metrics.</param>
        /// <param name="ignoreRequestPathPatterns">Patterns for paths to ignore.</param>
        /// <param name="owinContext">Name of the metrics context where to register the metrics.</param>
        /// <returns>Chainable configuration object.</returns>
        public OwinMetricsConfig WithRequestMetricsConfig(Action<OwinRequestMetricsConfig> config,
            Regex[] ignoreRequestPathPatterns = null, string owinContext = "Owin")
        {
            OwinRequestMetricsConfig requestConfig = new OwinRequestMetricsConfig(this.middlewareRegistration,
                this.context.Context(owinContext), ignoreRequestPathPatterns);

            config(requestConfig);

            return this;
        }

        /// <summary>
        /// Expose Owin metrics endpoint
        /// </summary>
        /// <returns>Chainable configuration object.</returns>
        public OwinMetricsConfig WithMetricsEndpoint()
        {
            WithMetricsEndpoint(_ => { });
            return this;
        }

        /// <summary>
        /// Configure Owin metrics endpoint.
        /// </summary>
        /// <param name="config">Action used to configure the Owin Metrics endpoint.</param>
        /// <returns>Chainable configuration object.</returns>
        public OwinMetricsConfig WithMetricsEndpoint(Action<OwinMetricsEndpointConfig> config)
        {
            OwinMetricsEndpointConfig endpointConfig = new OwinMetricsEndpointConfig();
            config(endpointConfig);
            var metricsEndpointMiddleware = new MetricsEndpointMiddleware(endpointConfig, this.context.DataProvider, this.healthStatus);
            this.middlewareRegistration(metricsEndpointMiddleware);
            return this;
        }
    }
}
