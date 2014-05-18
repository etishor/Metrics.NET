using System;
using Metrics;
using Metrics.Core;

namespace Nancy.Metrics
{
    public class NancyMetricsConfig
    {
        private readonly MetricsRegistry metricsRegistry;
        private readonly Func<HealthStatus> healthStatus;
        private NancyGlobalMetrics globalMetrics;

        public NancyMetricsConfig(MetricsRegistry metricsRegistry, Func<HealthStatus> healthStatus)
        {
            this.metricsRegistry = metricsRegistry;
            this.healthStatus = healthStatus;
        }

        public MetricsRegistry Registry { get { return this.metricsRegistry; } }
        //public HealthChecksRegistry HealthChecks { get { return this.healthChecksRegistry; } }

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
        public NancyMetricsConfig WithMetricsModule(string metricsPath = "/metrics")
        {
            return WithMetricsModule(m => { }, metricsPath);
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
        public NancyMetricsConfig WithMetricsModule(Action<INancyModule> moduleConfig, string metricsPath = "/metrics")
        {
            MetricsModule.Configure(this.metricsRegistry, this.healthStatus, moduleConfig, metricsPath);
            return this;
        }
    }
}
