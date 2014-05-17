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
            CurrentConfig = new NancyMetricsConfig(Metric.Config.Registry, HealthChecks.Registry);
            return config;
        }

        /// <summary>
        /// Configure NancyFx integration
        /// </summary>
        /// <param name="config">Chainable configuration object.</param>
        /// <param name="nancyConfig">Action to configure NancyFx integration.</param>
        /// <returns>Chainable configuration object.</returns>
        public static MetricsConfig WithNancy(this MetricsConfig config, Action<NancyMetricsConfig> nancyConfig)
        {
            CurrentConfig = new NancyMetricsConfig(Metric.Config.Registry, HealthChecks.Registry);
            nancyConfig(CurrentConfig);
            return config;
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
            CurrentConfig = new NancyMetricsConfig(registry, HealthChecks.Registry);
            nancyConfig(CurrentConfig);
            return config;
        }

        /// <summary>
        /// Configure NancyFx integration
        /// </summary>
        /// <param name="config">Chainable configuration object.</param>
        /// <param name="registry">Custom metrics registry.</param>
        /// <param name="healthChecksRegistry">Custom health checks registry</param>
        /// <param name="nancyConfig">Action to configure NancyFx integration.</param>
        /// <returns>Chainable configuration object.</returns>
        public static MetricsConfig WithNancy(this MetricsConfig config, MetricsRegistry registry, HealthChecksRegistry healthChecksRegistry,
            Action<NancyMetricsConfig> nancyConfig)
        {
            CurrentConfig = new NancyMetricsConfig(registry, healthChecksRegistry);
            nancyConfig(CurrentConfig);
            return config;
        }
    }
}
