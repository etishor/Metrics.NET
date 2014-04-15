using System;
using System.Threading;
using Metrics.Utils;

namespace Metrics.Reporters
{
    public sealed class ScheduledReporter : IDisposable
    {
        private readonly Timer reportTime = null;
        private readonly Scheduler scheduler;
        private readonly TimeSpan interval;

        private readonly Func<Reporter> reporter;
        private readonly MetricsRegistry registry;
        private readonly HealthChecksRegistry healthChecks;


        public ScheduledReporter(string name, Func<Reporter> reporter, MetricsRegistry registry, HealthChecksRegistry healthChecks, TimeSpan interval)
            : this(name, reporter, registry, healthChecks, interval, new ActionScheduler()) { }

        public ScheduledReporter(string name, Func<Reporter> reporter, MetricsRegistry registry, HealthChecksRegistry healthChecks, TimeSpan interval, Scheduler scheduler)
        {
            if (Metric.Reports.EnableReportDiagnosticMetrics)
            {
                this.reportTime = registry.Timer("Metrics.Reporter." + name, Unit.Calls, SamplingType.FavourRecent, TimeUnit.Seconds, TimeUnit.Milliseconds);
            }

            this.reporter = reporter;
            this.registry = registry;
            this.healthChecks = healthChecks;
            this.interval = interval;
            this.scheduler = scheduler;
        }

        private void RunReport(CancellationToken token)
        {
            using (this.reportTime != null ? this.reportTime.NewContext() : null)
            {
                reporter().RunReport(this.registry, this.healthChecks, token);
            }
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
