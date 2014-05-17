using Metrics;
using Metrics.Core;

namespace Nancy.Metrics
{
    /// <summary>
    /// Helper class to register a few NancyFx related metrics.
    /// </summary>
    public static class NancyMetrics
    {
        public static NancyMetricsConfig CurrentConfig { get; private set; }

        /// <summary>
        /// Start configuring metrics integration into NancyFx
        /// </summary>
        /// <returns>Instance that handles integration customizations</returns>
        public static NancyMetricsConfig Configure()
        {
            return Configure(Metric.Registry, HealthChecks.Registry);
        }

        /// <summary>
        /// Start configuring metrics integration into NancyFx
        /// </summary>
        /// <param name="metricsRegistry">Custom metrics registry</param>
        /// <returns>Instance that handles integration customizations</returns>
        public static NancyMetricsConfig Configure(MetricsRegistry registry)
        {
            return Configure(registry, HealthChecks.Registry);
        }

        /// <summary>
        /// Start configuring metrics integration into NancyFx with a custom <paramref name="metricsRegistry"/>
        /// </summary>
        /// <remarks>
        /// This method is useful for testing.
        /// </remarks>
        /// <param name="metricsRegistry">Custom metrics registry</param>
        /// <returns>Instance that handles integration customizations</returns>
        public static NancyMetricsConfig Configure(MetricsRegistry metricsRegistry, HealthChecksRegistry healthChecksRegistry)
        {
            CurrentConfig = new NancyMetricsConfig(metricsRegistry, healthChecksRegistry);
            return CurrentConfig;
        }
    }
}
