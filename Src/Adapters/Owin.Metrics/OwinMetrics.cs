using System;
using Metrics;

namespace Owin.Metrics
{
    public static class OwinMetrics
    {
        public static MetricsConfig WithOwin(this MetricsConfig config, Action<object> middlewareRegistration, Action<OwinMetricsConfig> owinConfig)
        {
            OwinMetricsConfig owin = new OwinMetricsConfig(middlewareRegistration, config.Registry, config.HealthStatus);
            owinConfig(owin);
            return config;
        }
    }
}
