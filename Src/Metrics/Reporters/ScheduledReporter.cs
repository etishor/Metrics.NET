using System;
using System.Threading;
using Metrics.Core;
using Metrics.Utils;

namespace Metrics.Reporters
{
    public sealed class ScheduledReporter : IDisposable
    {
        private readonly Scheduler scheduler;
        private readonly TimeSpan interval;

        private readonly Func<Reporter> reporter;
        private readonly MetricsData metricsData;
        private readonly Func<HealthStatus> healthStatus;

        public ScheduledReporter(string name, Func<Reporter> reporter, MetricsData metricsData, Func<HealthStatus> healthStatus, TimeSpan interval)
            : this(name, reporter, metricsData, healthStatus, interval, new ActionScheduler()) { }

        public ScheduledReporter(string name, Func<Reporter> reporter, MetricsData metricsData, Func<HealthStatus> healthStatus, TimeSpan interval, Scheduler scheduler)
        {
            this.reporter = reporter;
            this.metricsData = metricsData;
            this.healthStatus = healthStatus;
            this.interval = interval;
            this.scheduler = scheduler;
        }

        private void RunReport(CancellationToken token)
        {
            reporter().RunReport(this.metricsData, this.healthStatus, token);
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
            this.scheduler.Dispose();
        }
    }
}
