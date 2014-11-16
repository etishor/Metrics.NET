
using System;
using Metrics.NLog;
using Metrics.Reporters;
using Metrics.Reports;
namespace Metrics
{
    public static class NLogReportsConfigExtensions
    {
        /// <summary>
        /// Write CSV Metrics Reports using NLog. 
        /// </summary>
        /// <param name="reports">Instance to configure</param>
        /// <param name="interval">Interval at which to report values</param>
        /// <param name="delimiter">Delimiter to use for CSV</param>
        /// <returns>Same Reports instance for chaining</returns>
        public static MetricsReports WithNLogCSVReports(this MetricsReports reports, TimeSpan interval, string delimiter = CSVAppender.CommaDelimiter)
        {
            global::NLog.Config.ConfigurationItemFactory.Default.Layouts.RegisterDefinition("CsvGaugeLayout", typeof(CsvGaugeLayout));
            global::NLog.Config.ConfigurationItemFactory.Default.Layouts.RegisterDefinition("CsvCounterLayout", typeof(CsvCounterLayout));
            global::NLog.Config.ConfigurationItemFactory.Default.Layouts.RegisterDefinition("CsvMeterLayout", typeof(CsvMeterLayout));
            global::NLog.Config.ConfigurationItemFactory.Default.Layouts.RegisterDefinition("CsvHistogramLayout", typeof(CsvHistogramLayout));
            global::NLog.Config.ConfigurationItemFactory.Default.Layouts.RegisterDefinition("CsvTimerLayout", typeof(CsvTimerLayout));

            reports.WithReport(new CSVReport(new NLogCSVAppender(delimiter)), interval);

            return reports;
        }

        /// <summary>
        /// Write human readable text using NLog
        /// </summary>
        /// <param name="reports">Instance to configure</param>
        /// <param name="interval">Interval at which to report values</param>
        /// <returns>Same Reports instance for chaining</returns>
        public static MetricsReports WithNLogTextReports(this MetricsReports reports, TimeSpan interval)
        {
            reports.WithReport(new NLogTextReporter(), interval);
            return reports;
        }
    }
}
