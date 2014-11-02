using System;
using System.Threading;
using System.Threading.Tasks;
using Metrics.MetricData;

namespace Metrics.Reporters
{
    public interface MetricsReporter : Utils.IHideObjectMembers
    {
        void RunReport(MetricsData metricsData, Func<HealthStatus> healthStatus, CancellationToken token);
    }

    public interface AsyncMetricsReporter : Utils.IHideObjectMembers
    {
        Task RunReportAsync(MetricsData metricsData, Func<HealthStatus> healthStatus, CancellationToken token);
    }
}
