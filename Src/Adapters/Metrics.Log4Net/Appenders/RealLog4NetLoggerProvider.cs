using System;
using System.Globalization;
using log4net;
using log4net.Core;

namespace Metrics.Log4Net.Appenders
{
    public class RealLog4NetLoggerProvider : ILoggerProvider
    {
        public ILogger GetLogger(MetricsData metricsData)
        {
            if (metricsData == null) throw new ArgumentNullException("metricsData");

            var loggerName = string.Format(CultureInfo.InvariantCulture, "Metrics.CSV.{0}.{1}", metricsData.MetricType, metricsData.MetricName);
            var logger = LogManager.GetLogger(loggerName).Logger;

            return logger;
        }
    }
}