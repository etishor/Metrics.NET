using System;
using System.Threading;

namespace Metrics.Reporters
{
    public interface MetricsReporter : Utils.IHideObjectMembers
    {
        void RunReport(MetricsData metricsData, Func<HealthStatus> healthStatus, CancellationToken token);
    }
}
