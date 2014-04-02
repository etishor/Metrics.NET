using Metrics;

namespace Nancy.Metrics
{
    /// <summary>
    /// Helper class to register a few NancyFx related metrics.
    /// </summary>
    public static class NancyMetrics
    {
        /// <summary>
        /// Start configuring metrics integration into NancyFx
        /// </summary>
        /// <returns>Instance that handles integration customizations</returns>
        public static NancyMetricsConfig Configure()
        {
            return Configure(Metric.Registry);
        }

        /// <summary>
        /// Start configuring metrics integration into NancyFx with a custom <param name="metricsRegistry">Metrics Registry</param>
        /// </summary>
        /// <remarks>
        /// This method is useful for testing.
        /// </remarks>
        /// <param name="metricsRegistry">Custom metrics registry</param>
        /// <returns>Instance that handles integration customizations</returns>
        public static NancyMetricsConfig Configure(MetricsRegistry metricsRegistry)
        {
            return new NancyMetricsConfig(metricsRegistry);
        }
    }
}
