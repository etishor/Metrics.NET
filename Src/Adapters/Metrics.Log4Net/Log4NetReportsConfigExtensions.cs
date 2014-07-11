using System;
using Metrics.Log4Net;
using Metrics.Log4Net.Appenders;
using Metrics.Log4Net.Layout;
using Metrics.Reporters;
using Metrics.Reports;

namespace Metrics
{

    public static class Log4NetReportsConfigExtensions
    {

        /// <summary>
        /// Configures Metrics.Log4Net to use custom CSV delimiter
        /// </summary>
        /// <param name="reports"></param>
        /// <param name="csvDelimiter">CSV delimiter. System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator will be used if delimiter is not specified.</param>
        /// <returns>Same Reports instance for chaining</returns>
        public static MetricsReports WithCustomMetricsLog4NetCsvDelimiter(this MetricsReports reports, string csvDelimiter = null)
        {
            CsvDelimiter.Value = csvDelimiter ?? System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator;

            return reports;
        }

        /// <summary>
        /// Metrics.Log4Net loads configuration and watches for changes in Metrics.Log4Net.config file
        /// </summary>
        /// <param name="reports"></param>
        /// <param name="logDirectory"></param>
        /// <returns>Same Reports instance for chaining</returns>
        /// <remarks>Note that <see cref="WithCustomMetricsLog4NetCsvDelimiter"/> should be called before calling this method as Log4Net immediately will create header with default CSV delimiter.</remarks>
        public static MetricsReports WithDefaultMetricsLog4NetConfigFile(this MetricsReports reports, string logDirectory = @".\Logs\")
        {
            DefaultLog4NetConfiguration.ConfigureAndWatch(logDirectory);

            return reports;
        }

        /// <summary>
        /// Write CSV Metrics Reports using Log4Net. 
        /// </summary>
        /// <param name="reports">Instance to configure</param>
        /// <param name="interval">Interval at which to report values</param>
        /// <returns>Same Reports instance for chaining</returns>
        public static MetricsReports WithLog4NetCSVReports(this MetricsReports reports, TimeSpan interval)
        {
            if (reports == null) throw new ArgumentNullException("reports");

            reports.WithReporter("Log4Net CSV Report", () => new CSVReporter(new Log4NetCSVAppender(new RealLog4NetLoggerProvider(), new LoggingEventMapper())), interval);

            return reports;
        }


        /// <summary>
        /// Write human readable text using Log4Net
        /// </summary>
        /// <param name="reports">Instance to configure</param>
        /// <param name="interval">Interval at which to report values</param>
        /// <returns>Same Reports instance for chaining</returns>
        public static MetricsReports WithLog4NetTextReports(this MetricsReports reports, TimeSpan interval)
        {
            if (reports == null) throw new ArgumentNullException("reports");

            reports.WithReporter("Log4Net Text Report", () => new Log4NetTextReporter(), interval);
            return reports;
        }
    }
}
