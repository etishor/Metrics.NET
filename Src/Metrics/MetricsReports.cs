
using System;
using System.Collections.Generic;
using System.IO;
using Metrics.Reporters;
namespace Metrics
{
    public class MetricsReports
    {
        private readonly MetricsRegistry metricsRegistry;

        private readonly List<ScheduledReporter> reports = new List<ScheduledReporter>();

        public MetricsReports(MetricsRegistry metricsRegistry)
        {
            this.metricsRegistry = metricsRegistry;
        }

        public void PrintConsoleReport(TimeSpan interval)
        {
            var reporter = new ScheduledReporter("Console", () => new ConsoleReporter(), this.metricsRegistry, interval);
            reporter.Start();
            this.reports.Add(reporter);
        }

        public void StoreCSVReports(string directory, TimeSpan interval)
        {
            Directory.CreateDirectory(directory);
            var reporter = new ScheduledReporter("CSVFiles", () => new CSVReporter(new CSVFileAppender(directory)), this.metricsRegistry, interval);
            reporter.Start();
            this.reports.Add(reporter);
        }

        public void AppendToFile(string filePath, TimeSpan interval)
        {
            var reporter = new ScheduledReporter("TextFile", () => new TextFileReporter(filePath), this.metricsRegistry, interval);
            reporter.Start();
            this.reports.Add(reporter);
        }
    }
}
