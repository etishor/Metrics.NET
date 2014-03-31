
using System;
using System.Collections.Generic;
using System.IO;
using Metrics.Reporters;
namespace Metrics
{
    public class MetricsReports : Utils.IHideObjectMembers
    {
        private readonly MetricsRegistry metricsRegistry;

        private readonly List<ScheduledReporter> reports = new List<ScheduledReporter>();

        public MetricsReports(MetricsRegistry metricsRegistry)
        {
            this.metricsRegistry = metricsRegistry;
        }

        /// <summary>
        /// Schedule a Console Report to be executed and displayed on the console at a fixed <paramref name="interval"/>.
        /// </summary>
        /// <param name="interval">Interval at which to display the report on the Console.</param>
        public void PrintConsoleReport(TimeSpan interval)
        {
            var reporter = new ScheduledReporter("Console", () => new ConsoleReporter(), this.metricsRegistry, interval);
            reporter.Start();
            this.reports.Add(reporter);
        }

        /// <summary>
        /// Configure Metrics to append a line for each metric to a CSV file in the <paramref name="directory"/>.
        /// </summary>
        /// <param name="directory">Directory where to store the CSV files.</param>
        /// <param name="interval">Interval at which to append a line to the files.</param>
        public void StoreCSVReports(string directory, TimeSpan interval)
        {
            Directory.CreateDirectory(directory);
            var reporter = new ScheduledReporter("CSVFiles", () => new CSVReporter(new CSVFileAppender(directory)), this.metricsRegistry, interval);
            reporter.Start();
            this.reports.Add(reporter);
        }

        /// <summary>
        /// Schedule a Human Readable report to be executed and appended to a text file.
        /// </summary>
        /// <param name="filePath">File where to append the report.</param>
        /// <param name="interval">Interval at which to run the report.</param>
        public void AppendToFile(string filePath, TimeSpan interval)
        {
            var reporter = new ScheduledReporter("TextFile", () => new TextFileReporter(filePath), this.metricsRegistry, interval);
            reporter.Start();
            this.reports.Add(reporter);
        }
    }
}
