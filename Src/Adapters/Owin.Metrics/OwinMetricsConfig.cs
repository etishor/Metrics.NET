using Metrics;
using Metrics.Core;
using Owin.Metrics.Middleware;
using System;
using System.Text.RegularExpressions;
namespace Owin.Metrics
{
    public class OwinMetricsConfig
    {
        private readonly Action<object> middlewareRegistration;
        private readonly MetricsRegistry registry;
        private readonly Func<HealthStatus> healthStatus;

        public OwinMetricsConfig(Action<object> middlewareRegistration, MetricsRegistry registry, Func<HealthStatus> healthStatus)
        {
            this.middlewareRegistration = middlewareRegistration;
            this.registry = registry;
            this.healthStatus = healthStatus;
        }

        public OwinMetricsConfig WithRequestMetricsConfig(Action<OwinRequestMetricsConfig> config, Regex[] ignoreRequestPathPatterns = null)
        {
            OwinRequestMetricsConfig requestConfig = new OwinRequestMetricsConfig(this.middlewareRegistration, this.registry,
                ignoreRequestPathPatterns);
            config(requestConfig);
            return this;
        }

        public OwinMetricsConfig WithMetricsEndpoint()
        {
            WithMetricsEndpoint(_ => { });
            return this;
        }

        public OwinMetricsConfig WithMetricsEndpoint(Action<OwinMetricsEndpointConfig> config)
        {
            OwinMetricsEndpointConfig endpointConfig = new OwinMetricsEndpointConfig();
            config(endpointConfig);
            var metricsEndpointMiddleware = new MetricsEndpointMiddleware(endpointConfig, this.registry, this.healthStatus);
            this.middlewareRegistration(metricsEndpointMiddleware);
            return this;
        }
    }
}
