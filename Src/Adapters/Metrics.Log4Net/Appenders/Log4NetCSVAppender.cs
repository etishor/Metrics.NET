using System;
using System.Collections.Generic;
using Metrics.Log4Net.Layout;
using Metrics.Reporters;
using log4net.Core;

namespace Metrics.Log4Net.Appenders
{
    public class Log4NetCSVAppender : CSVAppender
    {
        private readonly ILoggerProvider loggerProvider;
        private readonly ILoggingEventMapper loggingEventMapper;

        public Log4NetCSVAppender(ILoggerProvider loggerProvider, ILoggingEventMapper loggingEventMapper)
            : base(CsvDelimiter.Value)
        {
            this.loggerProvider = loggerProvider;
            this.loggingEventMapper = loggingEventMapper;
        }

        public override void AppendLine(DateTime timestamp, string metricType, string metricName, IEnumerable<CSVReporter.Value> values)
        {
            var metricsData = new MetricsData { MetricType = metricType, MetricName = metricName, Values = values };

            var logger = loggerProvider.GetLogger(metricsData);

            if (!logger.IsEnabledFor(Level.Info)) return;

            var loggingEvent = loggingEventMapper.MapToLoggingEvent(logger.Name, timestamp, metricsData);

            logger.Log(loggingEvent);
        }
    }
}
