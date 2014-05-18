
using System;
using Metrics.Core;
using Metrics.Reports;
using Metrics.Visualization;

namespace Metrics
{
    public sealed class MetricsConfig : IDisposable, Utils.IHideObjectMembers
    {
        private bool isDisabled = false;

        private Lazy<MetricsRegistry> registry;
        private Func<HealthStatus> healthStatus;

        private readonly Lazy<MetricsReports> reports;
        private MetricsHttpListener listener;

        internal MetricsConfig()
        {
            this.registry = new Lazy<MetricsRegistry>(() => new LocalRegistry(), true);
            this.healthStatus = () => HealthChecks.GetStatus();
            this.reports = new Lazy<MetricsReports>(() => new MetricsReports(this.Registry, this.HealthStatus));
        }

        /// <summary>
        /// The registry where all the metrics are registered. 
        /// </summary>
        public MetricsRegistry Registry { get { return this.registry.Value; } }

        /// <summary>
        /// Function that provides the current health status.
        /// </summary>
        public Func<HealthStatus> HealthStatus { get { return this.healthStatus; } }

        /// <summary>
        /// All metrics operations will be NO-OP.
        /// This is usefull for measuring the impact of the metrics library on the application.
        /// If you think the Metrics library is causing issues, this will disable all Metrics operations.
        /// </summary>
        public void CompletelyDisableMetrics()
        {
            WithRegistry(new NullMetricsRegistry());
            this.isDisabled = true;
        }

        /// <summary>
        /// Create HTTP endpoint where metrics will be available in various formats:
        /// GET / => visualization application
        /// GET /json => metrics serialized as JSON
        /// GET /text => metrics in human readable text format
        /// </summary>
        /// <param name="httpUriPrefix">prefix where to start HTTP endpoint</param>
        public MetricsConfig WithHttpEndpoint(string httpUriPrefix)
        {
            using (this.listener) { }
            this.listener = new MetricsHttpListener(httpUriPrefix, this.Registry, this.healthStatus);
            this.listener.Start();
            return this;
        }


        /// <summary>
        /// Configure the Metrics library to use a custom MetricsRegistry.
        /// </summary>
        /// <remarks>
        /// You must call Metric.Config.WithRegistry before any other Metric call.
        /// </remarks>
        /// <param name="registry">The custom registry to use for registering metrics.</param>
        /// <returns>Chainable configuration object.</returns>
        public MetricsConfig WithRegistry(MetricsRegistry registry)
        {
            if (this.isDisabled)
            {
                return this;
            }

            if (this.registry.IsValueCreated)
            {
                throw new InvalidOperationException("Metrics registry has already been created. You must call Metric.Config.WithRegistry before any other Metric call.");
            }

            this.registry = new Lazy<MetricsRegistry>(() => registry);

            return this;
        }

        /// <summary>
        /// Configure Metrics library to use a custom health status reporter. By default HealthChecks.GetStatus() is used.
        /// </summary>
        /// <param name="healthStatus">Function that provides the current health status.</param>
        /// <returns>Chainable configuration object.</returns>
        public MetricsConfig WithHealthStatus(Func<HealthStatus> healthStatus)
        {
            if (this.registry.IsValueCreated)
            {
                throw new InvalidOperationException("Metrics registry has already been created. You must call Metric.Config.WithHealthStatus before any Report configuration other Metric call.");
            }
            this.healthStatus = healthStatus;
            return this;
        }

        /// <summary>
        /// Global error handler for the metrics library. If a handler is registered any error will be passed to the handler.
        /// </summary>
        /// <param name="errorHandler">Action with will be executed with the exception.</param>
        /// <returns>Chainable configuration object.</returns>
        public MetricsConfig WithErrorHandler(Action<Exception> errorHandler)
        {
            if (this.isDisabled)
            {
                return this;
            }

            this.ErrorHandler = errorHandler;

            return this;
        }

        /// <summary>
        /// Configure the way metrics are reported
        /// </summary>
        /// <param name="reportsConfig">Reports configuration action</param>
        /// <returns>Chainable configuration object.</returns>
        public MetricsConfig WithReporting(Action<MetricsReports> reportsConfig)
        {
            if (this.isDisabled)
            {
                return this;
            }

            reportsConfig(this.reports.Value);
            return this;
        }

        public void Dispose()
        {
            using (this.listener) { }
            this.listener = null;
        }

        /// <summary>
        /// Configured error handler
        /// </summary>
        internal Action<Exception> ErrorHandler { get; private set; }       
    }
}
