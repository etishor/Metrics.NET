﻿
using System;
using System.Collections.Generic;
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
        /// Schedule a generic reporter to be executed at a fixed <paramref name="interval"/>
        /// </summary>
        /// <param name="reporterName">Name of the reporter</param>
        /// <param name="reporter">Function that returns an instance of a reporter</param>
        /// <param name="interval">Interval at which to run the report.</param>
        public MetricsReports WithReporter(string reporterName, Func<Reporter> reporter, TimeSpan interval)
        {
            var report = new ScheduledReporter(reporterName, reporter, this.metricsRegistry, this.healthStatus, interval);
            report.Start();
            this.reports.Add(report);
            return this;
        }

        /// <summary>
        /// Schedule a Console Report to be executed and displayed on the console at a fixed <paramref name="interval"/>.
        /// </summary>
        /// <param name="interval">Interval at which to display the report on the Console.</param>
        public MetricsReports WithConsoleReport(TimeSpan interval)
        {
            return WithReporter("Console", () => new ConsoleReporter(), interval);
        }

        /// <summary>
        /// Configure Metrics to append a line for each metric to a CSV file in the <paramref name="directory"/>.
        /// </summary>
        /// <param name="directory">Directory where to store the CSV files.</param>
        /// <param name="interval">Interval at which to append a line to the files.</param>
        /// <param name="delimiter">CSV delimiter to use</param>
        public MetricsReports WithCSVReports(string directory, TimeSpan interval, string delimiter = CSVAppender.CommaDelimiter)
        {
            return WithReporter("CSVFiles", () => new CSVReporter(new CSVFileAppender(directory, delimiter)), interval);
        }

        ///// <summary>
        ///// Configure Metrics to append a line for each metric to a CSV file using the custom <paramref name="fileAppender"/>.
        ///// </summary>
        ///// <param name="fileAppender">Custom file appender to write the CSV files.</param>
        ///// <param name="interval">Interval at which to append a line to the files.</param>
        //public MetricsReports WithCSVReports(CSVFileAppender fileAppender, TimeSpan interval)
        //{
        //    return WithReporter("CSVFiles", () => new CSVReporter(fileAppender), interval);
        //}

        /// <summary>
        /// Schedule a Human Readable report to be executed and appended to a text file.
        /// </summary>
        /// <param name="filePath">File where to append the report.</param>
        /// <param name="interval">Interval at which to run the report.</param>
        public MetricsReports WithTextFileReport(string filePath, TimeSpan interval)
        {
            return WithReporter("TextFile", () => new TextFileReporter(filePath), interval);
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
