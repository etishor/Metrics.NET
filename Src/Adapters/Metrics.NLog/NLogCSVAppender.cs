using System;
using System.Collections.Generic;
using Metrics.Reporters;

namespace Metrics.NLog
{
    using System.IO;
    using LogEventInfo = global::NLog.LogEventInfo;
    using LogLevel = global::NLog.LogLevel;
    using LogManager = global::NLog.LogManager;

    public class NLogCSVAppender : CSVAppender
    {
        public NLogCSVAppender(string delimiter)
            : base(delimiter)
        { }

        public override void AppendLine(DateTime timestamp, string metricType, string metricName, IEnumerable<CSVReport.Value> values)
        {
            var loggerName = string.Format("Metrics.CSV.{0}.{1}", metricType, metricName);
            LogManager.GetLogger(loggerName).Log(GetLogEvent(loggerName, timestamp, metricType, metricName, values));
        }

        private LogEventInfo GetLogEvent(string logger, DateTime timestamp, string metricType, string metricName, IEnumerable<CSVReport.Value> values)
        {
            var logEvent = new LogEventInfo(LogLevel.Info, logger, GetValues(timestamp, values));
            logEvent.Properties.Add("MetricType", CleanFileName(metricType));
            logEvent.Properties.Add("MetricName", CleanFileName(metricName));
            logEvent.Properties.Add("Date", timestamp.ToString("u"));
            logEvent.Properties.Add("Ticks", timestamp.Ticks.ToString("D"));
            foreach (var value in values)
            {
                logEvent.Properties.Add(value.Name, value.FormattedValue);
            }
            return logEvent;
        }

        protected virtual string CleanFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            foreach (var c in invalid)
            {
                name = name.Replace(c, '_');
            }
            return name;
        }
    }
}
