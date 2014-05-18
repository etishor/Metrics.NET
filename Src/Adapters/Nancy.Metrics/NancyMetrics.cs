using System;
using Metrics.Core;
using Nancy.Metrics;

namespace Metrics
{
    /// <summary>
    /// Helper class to register a few NancyFx related metrics.
    /// </summary>
    public static class NancyMetrics
    {
        internal static NancyMetricsConfig CurrentConfig { get; private set; }

        /// <summary>
        /// Configure NancyFx integration
        /// </summary>
        /// <param name="config">Chainable configuration object.</param>
        /// <returns>Chainable configuration object.</returns>
        public static MetricsConfig WithNancy(this MetricsConfig config)
        {
            return config.WithNancy(c => { });
        }

        /// <summary>
        /// Configure NancyFx integration
        /// </summary>
        /// <param name="config">Chainable configuration object.</param>
        /// <param name="nancyConfig">Action to configure NancyFx integration.</param>
        /// <returns>Chainable configuration object.</returns>
        public static MetricsConfig WithNancy(this MetricsConfig config, Action<NancyMetricsConfig> nancyConfig)
        {
            return config.WithNancy(config.Registry, nancyConfig);
        }

        /// <summary>
        /// Configure NancyFx integration
        /// </summary>
        /// <param name="config">Chainable configuration object.</param>
        /// <param name="registry">Custom metrics registry.</param>
        /// <param name="nancyConfig">Action to configure NancyFx integration.</param>
        /// <returns>Chainable configuration object.</returns>
        public static MetricsConfig WithNancy(this MetricsConfig config, MetricsRegistry registry, Action<NancyMetricsConfig> nancyConfig)
        {
            return config.WithNancy(registry, config.HealthStatus, nancyConfig);
        }

        /// <summary>
        /// Configure NancyFx integration
        /// </summary>
        /// <param name="config">Chainable configuration object.</param>
        /// <param name="registry">Custom metrics registry.</param>
        /// <param name="healthStatus">Custom health checks status</param>
        /// <param name="nancyConfig">Action to configure NancyFx integration.</param>
        /// <returns>Chainable configuration object.</returns>
        public static MetricsConfig WithNancy(this MetricsConfig config, MetricsRegistry registry, Func<HealthStatus> healthStatus,
            Action<NancyMetricsConfig> nancyConfig)
        {
            CurrentConfig = new NancyMetricsConfig(registry, healthStatus);
            nancyConfig(CurrentConfig);
            return config;
        }
    }
}
