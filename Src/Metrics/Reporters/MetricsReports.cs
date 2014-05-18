
using System;
using System.Collections.Generic;
using System.IO;
using Metrics.Core;
using Metrics.Reporters;
namespace Metrics.Reports
{
    public sealed class MetricsReports : Utils.IHideObjectMembers, IDisposable
    {
        private readonly MetricsRegistry metricsRegistry;
        private readonly Func<HealthStatus> healthStatus;

        private readonly List<ScheduledReporter> reports = new List<ScheduledReporter>();

        public MetricsReports(MetricsRegistry metricsRegistry, Func<HealthStatus> healthStatus)
        {
            this.metricsRegistry = metricsRegistry;
            this.healthStatus = healthStatus;
        }

        /// <summary>
        /// Enable or disable the registration of Report timer metrics.
        /// </summary>
        public bool EnableReportDiagnosticMetrics { get; set; }

        /// <summary>
        /// Schedule a Console Report to be executed and displayed on the console at a fixed <paramref name="interval"/>.
        /// </summary>
        /// <param name="interval">Interval at which to display the report on the Console.</param>
        public MetricsReports WithConsoleReport(TimeSpan interval)
        {
            var reporter = new ScheduledReporter("Console", () => new ConsoleReporter(), this.metricsRegistry, this.healthStatus, interval);
            reporter.Start();
            this.reports.Add(reporter);
            return this;
        }

        /// <summary>
        /// Configure Metrics to append a line for each metric to a CSV file in the <paramref name="directory"/>.
        /// </summary>
        /// <param name="directory">Directory where to store the CSV files.</param>
        /// <param name="interval">Interval at which to append a line to the files.</param>
        public MetricsReports WithCSVReports(string directory, TimeSpan interval)
        {
            Directory.CreateDirectory(directory);
            var reporter = new ScheduledReporter("CSVFiles", () => new CSVReporter(new CSVFileAppender(directory)), this.metricsRegistry, this.healthStatus, interval);
            reporter.Start();
            this.reports.Add(reporter);
            return this;
        }

        /// <summary>
        /// Schedule a Human Readable report to be executed and appended to a text file.
        /// </summary>
        /// <param name="filePath">File where to append the report.</param>
        /// <param name="interval">Interval at which to run the report.</param>
        public MetricsReports WithTextFileReport(string filePath, TimeSpan interval)
        {
            var reporter = new ScheduledReporter("TextFile", () => new TextFileReporter(filePath), this.metricsRegistry, this.healthStatus, interval);
            reporter.Start();
            this.reports.Add(reporter);
            return this;
        }

        /// <summary>
        /// Stop all registered reports and clear the registrations.
        /// </summary>
        public void StopAndClearAllReports()
        {
            this.reports.ForEach(r => r.Stop());
            this.reports.Clear();
        }

        public void Dispose()
        {
            this.reports.ForEach(r => r.Dispose());
            this.reports.Clear();
        }
    }
}
