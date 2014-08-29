
using System;
using Metrics.Reports;
using Metrics.Visualization;

namespace Metrics
{
    public sealed class MetricsConfig : IDisposable, Utils.IHideObjectMembers
    {
        private bool isDisabled = false;

        private Lazy<MetricContext> context;
        private Func<HealthStatus> healthStatus;

        private readonly Lazy<MetricsReports> reports;
        private MetricsHttpListener listener;

        internal MetricsConfig(MetricContext context)
        {
            this.context = new Lazy<MetricContext>(() => context, true);
            this.healthStatus = () => HealthChecks.GetStatus();
            this.reports = new Lazy<MetricsReports>(() => new MetricsReports(this.context.Value.MetricsData, this.HealthStatus));
        }

        /// <summary>
        /// The registry where all the metrics are registered. 
        /// </summary>
        public MetricContext Context { get { return this.context.Value; } }

        /// <summary>
        /// Function that provides the current health status.
        /// </summary>
        public Func<HealthStatus> HealthStatus { get { return this.healthStatus; } }

        /// <summary>
        /// All metrics operations will be NO-OP.
        /// This is useful for measuring the impact of the metrics library on the application.
        /// If you think the Metrics library is causing issues, this will disable all Metrics operations.
        /// </summary>
        public void CompletelyDisableMetrics()
        {
            //WithContext(new NullMetricsRegistry());
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
            if (this.isDisabled)
            {
                return this;
            }

            using (this.listener) { }
            this.listener = new MetricsHttpListener(httpUriPrefix, this.context.Value.MetricsData, this.healthStatus);
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
        public MetricsConfig WithContext(MetricContext context)
        {
            if (this.isDisabled)
            {
                return this;
            }

            if (this.context.IsValueCreated)
            {
                throw new InvalidOperationException("MetricsContext has already been created. You must call Metric.Config.WithContext before any other Metric call.");
            }

            this.context = new Lazy<MetricContext>(() => context);

            return this;
        }

        /// <summary>
        /// Configure Metrics library to use a custom health status reporter. By default HealthChecks.GetStatus() is used.
        /// </summary>
        /// <param name="healthStatus">Function that provides the current health status.</param>
        /// <returns>Chain-able configuration object.</returns>
        public MetricsConfig WithHealthStatus(Func<HealthStatus> healthStatus)
        {
            if (this.context.IsValueCreated)
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
        /// <returns>Chain able configuration object.</returns>
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
        /// <returns>Chain-able configuration object.</returns>
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
            if (this.context.IsValueCreated)
            {
                this.context.Value.Dispose();
            }

            if (this.reports.IsValueCreated)
            {
                this.reports.Value.StopAndClearAllReports();
                this.reports.Value.Dispose();
            }

            using (this.listener) { }
            this.listener = null;
        }

        /// <summary>
        /// Configured error handler
        /// </summary>
        internal Action<Exception> ErrorHandler { get; private set; }
    }
}
