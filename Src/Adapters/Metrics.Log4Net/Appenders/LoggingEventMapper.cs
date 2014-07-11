using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Metrics.Reporters;
using log4net.Core;

namespace Metrics.Log4Net.Appenders
{
    public class LoggingEventMapper : ILoggingEventMapper
    {
        public LoggingEvent MapToLoggingEvent(string loggerName, DateTime timestamp, MetricsData metricsData)
        {
            if (loggerName == null) throw new ArgumentNullException("loggerName");
            if (metricsData == null) throw new ArgumentNullException("metricsData");

            var listOfValues = metricsData.Values as IList<CSVReporter.Value> ?? metricsData.Values.ToList();

            var logEvent = new LoggingEvent(new LoggingEventData
              {
                  Level = Level.Info,
                  LoggerName = loggerName
              });

            logEvent.Properties["MetricType"] = metricsData.MetricType;
            logEvent.Properties["MetricName"] = metricsData.MetricName;
            logEvent.Properties["Date"] = timestamp.ToString("u", CultureInfo.InvariantCulture);
            logEvent.Properties["Ticks"] = timestamp.Ticks.ToString("D", CultureInfo.InvariantCulture);

            foreach (var value in listOfValues)
            {
                logEvent.Properties[value.Name] = value.FormattedValue;
            }

            return logEvent;
        }
    }
}