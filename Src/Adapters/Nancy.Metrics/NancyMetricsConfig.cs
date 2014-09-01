using System;
using Metrics;

namespace Nancy.Metrics
{
    public class NancyMetricsConfig
    {
        private readonly MetricContext metricsContext;
        private readonly Func<HealthStatus> healthStatus;
        private NancyGlobalMetrics globalMetrics;

        public NancyMetricsConfig(MetricContext metricsContext, Func<HealthStatus> healthStatus)
        {
            this.metricsContext = metricsContext;
            this.healthStatus = healthStatus;
        }

        public MetricContext Context { get { return this.metricsContext; } }

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
        public NancyMetricsConfig WithNancyMetrics(Action<NancyGlobalMetrics> config)
        {
            this.globalMetrics = new NancyGlobalMetrics(this.metricsContext);
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
            MetricsModule.Configure(this.metricsContext.MetricsData, this.healthStatus, moduleConfig, metricsPath);
            return this;
        }

        /// <summary>
        /// Make the Health Checks endpoint return HTTP Status 200 even if checks fail.
        /// </summary>
        /// <returns>This instance to allow chaining of the configuration.</returns>
        public NancyMetricsConfig WithHealthChecksThatAlwaysReturnHttpStatusOk()
        {
            MetricsModule.ConfigureHealthChecks(alwaysReturnOk: true);
            return this;
        }
    }
}
