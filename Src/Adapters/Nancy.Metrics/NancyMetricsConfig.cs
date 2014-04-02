using System;
using Metrics;

namespace Nancy.Metrics
{
    public class NancyMetricsConfig
    {
        private readonly MetricsRegistry metricsRegistry;
        private NancyGlobalMetrics globalMetrics;

        public NancyMetricsConfig(MetricsRegistry metricsRegistry)
        {
            this.metricsRegistry = metricsRegistry;
        }

        public MetricsRegistry Registry { get { return this.metricsRegistry; } }

        /// <summary>
        /// Configure global NancyFx Metrics.
        /// Available global metrics are: Request Timer, Active Requests Counter, Error Meter
        /// <code>
        /// protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        /// {
        ///     base.ApplicationStartup(container, pipelines);
        /// 
        ///     NancyMetrics.Configure()
        ///         .WithGlobalMetrics(config => config.RegisterAllMetrics(pipelines))
        ///         .WithMetricsEndpoint();
        /// }
        /// </code>
        /// </summary>
        /// <param name="config">Action to configure which global metrics to enable</param>
        /// <returns>This instance to allow chaining of the configuration.</returns>
        public NancyMetricsConfig WithGlobalMetrics(Action<NancyGlobalMetrics> config)
        {
            this.globalMetrics = new NancyGlobalMetrics(this.metricsRegistry);
            config(this.globalMetrics);
            return this;
        }

        /// <summary>
        /// Expose the metrics information at:
        /// /metrics in human readable format
        /// /metrics/json in json format
        /// <code>
        /// protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        /// {
        ///     base.ApplicationStartup(container, pipelines);
        /// 
        ///     NancyMetrics.Configure()
        ///         .WithGlobalMetrics(config => config.RegisterAllMetrics(pipelines))
        ///         .WithMetricsEndpoint(m => m.RequiresAuthentication()); // to enable authentication
        /// }
        /// </code>
        /// </summary>
        /// <param name="metricsPath">Path where to expose the metrics</param>
        /// <returns>This instance to allow chaining of the configuration.</returns>
        public NancyMetricsConfig WithMetricsEndpoint(string metricsPath = "/metrics")
        {
            return WithMetricsEndpoint(m => { }, metricsPath);
        }

        /// <summary>
        /// Expose the metrics information at:
        /// /metrics in human readable format
        /// /metrics/json in json format
        /// <code>
        /// protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        /// {
        ///     base.ApplicationStartup(container, pipelines);
        /// 
        ///     NancyMetrics.Configure()
        ///         .WithGlobalMetrics(config => config.RegisterAllMetrics(pipelines))
        ///         .WithMetricsEndpoint(m => m.RequiresAuthentication()); // to enable authentication
        /// }
        /// </code>
        /// </summary>
        /// <param name="moduleConfig">Action that can configure the Metrics Module ( for example to apply authentication )</param>
        /// <param name="metricsPath">Path where to expose the metrics</param>
        /// <returns>This instance to allow chaining of the configuration.</returns>
        public NancyMetricsConfig WithMetricsEndpoint(Action<INancyModule> moduleConfig, string metricsPath = "/metrics")
        {
            MetricsModule.Configure(this.metricsRegistry, moduleConfig, metricsPath);
            return this;
        }
    }
}
