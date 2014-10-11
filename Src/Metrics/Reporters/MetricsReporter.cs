using System;
using System.Threading;
using Metrics.MetricData;

namespace Metrics.Reporters
{
    public interface MetricsReporter : Utils.IHideObjectMembers
    {
        void RunReport(MetricsData metricsData, Func<HealthStatus> healthStatus, CancellationToken token);
    }
}
