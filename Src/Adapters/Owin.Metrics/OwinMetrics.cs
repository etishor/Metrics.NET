using Metrics.Core;
using Owin;
using Owin.Metrics;
using System;

namespace Metrics
{
    /// <summary>
    /// Helper class to register a few NancyFx related metrics.
    /// </summary>
    public static class OwinMetrics
    {
        internal static OwinMetricsConfig CurrentConfig { get; private set; }

        /// <summary>
        /// Configure NancyFx integration
        /// </summary>
        /// <param name="config">Chainable configuration object.</param>
        /// <param name="app">The application.</param>
        /// <param name="owinMetricsConfig">Action to configure WebApi integration.</param>
        /// <returns>
        /// Chainable configuration object.
        /// </returns>
        public static MetricsConfig WithWeb(this MetricsConfig config, IAppBuilder app, Action<OwinMetricsConfig> owinMetricsConfig)
        {
            return config.WithWeb(app, config.Registry, owinMetricsConfig);
        }

        /// <summary>
        /// Configure WebApi integration
        /// </summary>
        /// <param name="config">Chainable configuration object.</param>
        /// <param name="app">The application.</param>
        /// <param name="registry">Custom metrics registry.</param>
        /// <param name="webMetricsConfig">Action to configure WebApi integration.</param>
        /// <returns>
        /// Chainable configuration object.
        /// </returns>
        public static MetricsConfig WithWeb(this MetricsConfig config, IAppBuilder app,
            MetricsRegistry registry, Action<OwinMetricsConfig> webMetricsConfig)
        {
            CurrentConfig = new OwinMetricsConfig(app, registry);
            webMetricsConfig(CurrentConfig);
            return config;
        }
    }
}
