using System;
using System.Threading;
using System.Threading.Tasks;
using Metrics.MetricData;
using Metrics.Utils;

namespace Metrics.Reporters
{
    public sealed class AsyncScheduledReporter : IDisposable
    {
        private readonly Scheduler scheduler;
        private readonly TimeSpan interval;

        private readonly Func<AsyncMetricsReporter> reporter;
        private readonly MetricsDataProvider metricsDataProvider;
        private readonly Func<HealthStatus> healthStatus;

        public AsyncScheduledReporter(string name, Func<AsyncMetricsReporter> reporter, MetricsDataProvider metricsDataProvider, Func<HealthStatus> healthStatus, TimeSpan interval)
            : this(name, reporter, metricsDataProvider, healthStatus, interval, new ActionScheduler()) { }

        public AsyncScheduledReporter(string name, Func<AsyncMetricsReporter> reporter, MetricsDataProvider metricsDataProvider, Func<HealthStatus> healthStatus, TimeSpan interval, Scheduler scheduler)
        {
            this.reporter = reporter;
            this.metricsDataProvider = metricsDataProvider;
            this.healthStatus = healthStatus;
            this.interval = interval;
            this.scheduler = scheduler;
        }

        private Task RunReport(CancellationToken token)
        {
            return reporter().RunReportAsync(this.metricsDataProvider.CurrentMetricsData, this.healthStatus, token);
        }

        public void Start()
        {
            this.scheduler.Start(this.interval, t => RunReport(t));
        }

        public void Stop()
        {
            this.scheduler.Stop();
        }

        public void Dispose()
        {
            this.Stop();
            this.scheduler.Dispose();
        }
    }
}
