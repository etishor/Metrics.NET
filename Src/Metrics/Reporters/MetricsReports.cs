using System;
using System.Collections.Generic;
using System.IO;
using Metrics.Core;
using Metrics.Reporters;
namespace Metrics.Reports
{
    public class MetricsReports : Utils.IHideObjectMembers, IDisposable
    {
        public readonly MetricsRegistry MetricsRegistry;
        public readonly Func<HealthStatus> HealthStatus;

        public readonly List<IScheduledReporter> Reports = new List<IScheduledReporter>();

        public MetricsReports(MetricsRegistry metricsRegistry, Func<HealthStatus> healthStatus)
        {
            this.MetricsRegistry = metricsRegistry;
            this.HealthStatus = healthStatus;
        }

        /// <summary>
        /// Schedule a Console Report to be executed and displayed on the console at a fixed <paramref name="interval"/>.
        /// </summary>
        /// <param name="interval">Interval at which to display the report on the Console.</param>
        public MetricsReports WithConsoleReport(TimeSpan interval)
        {
            var reporter = new ScheduledReporter("Console", () => new ConsoleReporter(), this.MetricsRegistry, this.HealthStatus, interval);
            reporter.Start();
            this.Reports.Add(reporter);
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
            var reporter = new ScheduledReporter("CSVFiles", () => new CSVReporter(new CSVFileAppender(directory)), this.MetricsRegistry, this.HealthStatus, interval);
            reporter.Start();
            this.Reports.Add(reporter);
            return this;
        }


        /// <summary>
        /// Schedule a Human Readable report to be executed and appended to a text file.
        /// </summary>
        /// <param name="filePath">File where to append the report.</param>
        /// <param name="interval">Interval at which to run the report.</param>
        public MetricsReports WithTextFileReport(string filePath, TimeSpan interval)
        {
            var reporter = new ScheduledReporter("TextFile", () => new TextFileReporter(filePath), this.MetricsRegistry, this.HealthStatus, interval);
            reporter.Start();
            this.Reports.Add(reporter);
            return this;
        }

        /// <summary>
        /// Stop all registered reports and clear the registrations.
        /// </summary>
        public void StopAndClearAllReports()
        {
            this.Reports.ForEach(r => r.Stop());
            this.Reports.Clear();
        }

        public void Dispose()
        {
            this.Reports.ForEach(r => r.Dispose());
            this.Reports.Clear();
        }
    }
}
