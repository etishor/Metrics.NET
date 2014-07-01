using System;
using System.Threading;
using Metrics.Core;
using Metrics.Utils;

namespace Metrics.Reporters
{
  public class ScheduledReporter : IScheduledReporter
  {
        private readonly Scheduler scheduler;
        private readonly TimeSpan interval;

        private readonly Func<Reporter> reporter;
        private readonly MetricsRegistry registry;
        private readonly Func<HealthStatus> healthStatus;

        public ScheduledReporter(string name, Func<Reporter> reporter, MetricsRegistry registry, Func<HealthStatus> healthStatus, TimeSpan interval)
            : this(name, reporter, registry, healthStatus, interval, new ActionScheduler()) { }

        public ScheduledReporter(string name, Func<Reporter> reporter, MetricsRegistry registry, Func<HealthStatus> healthStatus, TimeSpan interval, Scheduler scheduler)
        {
            this.reporter = reporter;
            this.registry = registry;
            this.healthStatus = healthStatus;
            this.interval = interval;
            this.scheduler = scheduler;
        }

        public void RunReport(CancellationToken token)
        {
            reporter().RunReport(this.registry, this.healthStatus, token);
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
