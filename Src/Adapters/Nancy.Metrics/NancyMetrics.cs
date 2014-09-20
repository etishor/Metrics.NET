using System;
using Nancy.Bootstrapper;
using Nancy.Metrics;

namespace Metrics
{
    /// <summary>
    /// Helper class to register a few NancyFx related metrics.
    /// </summary>
    public static class NancyMetrics
    {
        /// <summary>
        /// Configure NancyFx integration
        /// </summary>
        /// <param name="config">Chainable configuration object.</param>
        /// <param name="nancyPipelines">Pipelines to hook on.</param>
        /// <returns>Chainable configuration object.</returns>
        public static MetricsConfig WithNancy(this MetricsConfig config, IPipelines nancyPipelines)
        {
            return config.WithNancy(nancyPipelines, nancy => nancy
                .WithNancyMetrics(m => m.WithAllMetrics())
                .WithMetricsModule()
            );
        }

        /// <summary>
        /// Configure NancyFx integration
        /// </summary>
        /// <param name="config">Chainable configuration object.</param>
        /// <param name="nancyPipelines">Pipelines to hook on.</param>
        /// <param name="nancyConfig">Nancy specific configuration options.</param>
        /// <returns>Chainable configuration object.</returns>
        public static MetricsConfig WithNancy(this MetricsConfig config, IPipelines nancyPipelines,
            Action<NancyMetricsConfig> nancyConfig)
        {
            var currentConfig = config.WithConfigExtension((ctx, hs) => new NancyMetricsConfig(ctx, hs, nancyPipelines));
            nancyConfig(currentConfig);
            return config;
        }
    }
}
