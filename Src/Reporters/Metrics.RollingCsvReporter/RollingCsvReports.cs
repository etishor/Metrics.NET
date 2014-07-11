using System;
using System.IO;
using Metrics.Reporters;
using Metrics.Reports;

namespace Metrics.RollingCsvReporter
{
  public static class RollingCsvReports
  {

    public static MetricsReports WithRollingCSVReports(this MetricsReports metricsReports, string directory, TimeSpan interval, int maxFileSizeBytes, int rollBackups, string delimiter)
    {
      Directory.CreateDirectory(directory);
      var reporter = new ScheduledReporter("RollingCSVFiles", () => new CSVReporter(new RollingCsvFileAppender(directory, new RuntimeConfiguredCsvRollingLogger(maxFileSizeBytes, rollBackups).GetLogger, delimiter)),
                                           metricsReports.MetricsRegistry, metricsReports.HealthStatus, interval);
      reporter.Start();
      metricsReports.Reports.Add(reporter);
      return metricsReports;
    }
  }
}