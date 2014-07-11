using log4net.Core;

namespace Metrics.Log4Net.Appenders
{
    public interface ILoggerProvider
    {
        ILogger GetLogger(MetricsData metricsData);
    }
}