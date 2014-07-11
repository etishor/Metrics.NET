using System;
using log4net.Core;

namespace Metrics.Log4Net.Appenders
{
    public interface ILoggingEventMapper
    {
        LoggingEvent MapToLoggingEvent(string loggerName, DateTime timestamp, MetricsData metricsData);
    }
}