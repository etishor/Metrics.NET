using System;
using Metrics;

namespace Owin.Metrics
{
    public static class OwinMetrics
    {
        public static MetricsConfig WithOwin(this MetricsConfig config, Action<object> middlewareRegistration, Action<OwinMetricsConfig> owinConfig)
        {
            var owin = config.Configure((ctx, hs) => new OwinMetricsConfig(middlewareRegistration, ctx, hs));
            owinConfig(owin);
            return config;
        }
    }
}
