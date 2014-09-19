
using System;
using System.Configuration;
using Metrics.Reports;
using Metrics.Visualization;

namespace Metrics
{
    public sealed class MetricsConfig : IDisposable, Utils.IHideObjectMembers
    {
        private readonly MetricsContext context;
        private readonly MetricsReports reports;

        private Func<HealthStatus> healthStatus;
        private MetricsHttpListener listener;

        private static readonly bool globalyDisabled = false;

        private bool isDisabled = MetricsConfig.globalyDisabled;

        static MetricsConfig()
        {
            globalyDisabled = ConfigureMetricsEnabledDisabled();
        }

        public MetricsConfig(MetricsContext context)
        {
            this.context = context;

            if (!globalyDisabled)
            {
                this.healthStatus = () => HealthChecks.GetStatus();
                this.reports = new MetricsReports(this.context.DataProvider, this.healthStatus);
                this.context.Advanced.ContextDisabled += (s, e) =>
                {
                    this.isDisabled |= true;
                    this.DisableAllReports();
                };
            }
        }

        /// <summary>
        /// Create HTTP endpoint where metrics will be available in various formats:
        /// GET / => visualization application
        /// GET /json => metrics serialized as JSON
        /// GET /text => metrics in human readable text format
        /// </summary>
        /// <param name="httpUriPrefix">prefix where to start HTTP endpoint</param>
        /// <returns>Chain-able configuration object.</returns>
        public MetricsConfig WithHttpEndpoint(string httpUriPrefix)
        {
            if (!isDisabled)
            {
                using (this.listener) { }
                this.listener = new MetricsHttpListener(httpUriPrefix, this.context.DataProvider, this.healthStatus);
                this.listener.Start();
            }
            return this;
        }

        /// <summary>
        /// Configure Metrics library to use a custom health status reporter. By default HealthChecks.GetStatus() is used.
        /// </summary>
        /// <param name="healthStatus">Function that provides the current health status.</param>
        /// <returns>Chain-able configuration object.</returns>
        public MetricsConfig WithHealthStatus(Func<HealthStatus> healthStatus)
        {
            if (!isDisabled)
            {
                this.healthStatus = healthStatus;
            }
            return this;
        }

        /// <summary>
        /// Error handler for the metrics library. If a handler is registered any error will be passed to the handler.
        /// </summary>
        /// <param name="errorHandler">Action with will be executed with the exception.</param>
        /// <returns>Chain able configuration object.</returns>
        public MetricsConfig WithErrorHandler(Action<Exception> errorHandler)
        {
            if (!isDisabled)
            {
                this.ErrorHandler = errorHandler;
            }
            return this;
        }

        /// <summary>
        /// Configure the way metrics are reported
        /// </summary>
        /// <param name="reportsConfig">Reports configuration action</param>
        /// <returns>Chain-able configuration object.</returns>
        public MetricsConfig WithReporting(Action<MetricsReports> reportsConfig)
        {
            if (!isDisabled)
            {
                reportsConfig(this.reports);
            }

            return this;
        }

        /// <summary>
        /// This method is used for customizing the metrics configuration.
        /// The <paramref name="extension"/> will be called with the current MetricsContext and HealthStatus provider.
        /// </summary>
        /// <remarks>
        /// In general you don't need to call this method directly.
        /// </remarks>
        /// <param name="extension">Action to apply extra configuration.</param>
        /// <returns>Chain-able configuration object.</returns>
        public MetricsConfig WithConfigExtension(Action<MetricsContext, Func<HealthStatus>> extension)
        {
            return WithConfigExtension((m, h) => { extension(m, h); return this; });
        }

        /// <summary>
        /// This method is used for customizing the metrics configuration.
        /// The <paramref name="extension"/> will be called with the current MetricsContext and HealthStatus provider.
        /// </summary>
        /// <remarks>
        /// In general you don't need to call this method directly.
        /// </remarks>
        /// <param name="extension">Action to apply extra configuration.</param>
        /// <returns>The result of calling the extension.</returns>
        public T WithConfigExtension<T>(Func<MetricsContext, Func<HealthStatus>, T> extension)
        {
            return extension(this.context, this.healthStatus);
        }

        public void Dispose()
        {
            this.reports.Dispose();
            using (this.listener) { }
            this.listener = null;
        }

        private void DisableAllReports()
        {
            this.reports.StopAndClearAllReports();
            using (this.listener) { }
            this.listener = null;
        }

        /// <summary>
        /// Configured error handler
        /// </summary>
        internal Action<Exception> ErrorHandler { get; private set; }

        internal void ApplySettingsFromConfigFile()
        {
            if (!globalyDisabled)
            {
                ConfigureCsvReports();
                ConfigureHttpListener();
            }
        }

        private void ConfigureHttpListener()
        {
            try
            {
                var httpEndpoint = ConfigurationManager.AppSettings["Metrics.HttpListener.HttpUriPrefix"];
                if (!string.IsNullOrEmpty(httpEndpoint))
                {
                    this.WithHttpEndpoint(httpEndpoint);
                }
            }
            catch (Exception x)
            {
                throw new InvalidOperationException("Invalid Metrics Configuration: Metrics.HttpListener.HttpUriPrefix muse be a valid HttpListener endpoint prefix", x);
            }
        }

        private void ConfigureCsvReports()
        {
            try
            {
                var csvMetricsPath = ConfigurationManager.AppSettings["Metrics.CSV.Path"];
                var csvMetricsInterval = ConfigurationManager.AppSettings["Metrics.CSV.Interval.Seconds"];

                if (!string.IsNullOrEmpty(csvMetricsPath) && !string.IsNullOrEmpty(csvMetricsInterval))
                {
                    int seconds;
                    if (int.TryParse(csvMetricsInterval, out seconds) && seconds > 0)
                    {
                        this.WithReporting(r => r.WithCSVReports(csvMetricsPath, TimeSpan.FromSeconds(seconds)));
                    }
                }
            }
            catch (Exception x)
            {
                throw new InvalidOperationException("Invalid Metrics Configuration: Metrics.CSV.Path muse be a valid path and Metrics.CSV.Interval.Seconds must be an integer > 0 ", x);
            }
        }

        private static bool ConfigureMetricsEnabledDisabled()
        {
            try
            {
                var isDisabled = ConfigurationManager.AppSettings["Metrics.CompetelyDisableMetrics"];
                if (!string.IsNullOrEmpty(isDisabled) && isDisabled.ToUpperInvariant() == "TRUE")
                {
                    Metric.Advanced.CompletelyDisableMetrics();
                    return true;
                }
                return false;
            }
            catch (Exception x)
            {
                throw new InvalidOperationException("Invalid Metrics Configuration: Metrics.CompetelyDisableMetrics must be set to true or false", x);
            }
        }
    }
}
