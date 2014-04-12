using System;
using System.Threading;
using Metrics.Utils;

namespace Metrics.Reporters
{
    public sealed class ScheduledReporter : IDisposable
    {
        private readonly Timer reportTime = null;
        private readonly ActionScheduler scheduler;
        private readonly TimeSpan interval;

        private readonly Func<Reporter> reporter;
        private readonly MetricsRegistry registry;

        public ScheduledReporter(string name, Func<Reporter> reporter, MetricsRegistry registry, TimeSpan interval)
        {
            if (Metric.Reports.EnableReportDiagnosticMetrics)
            {
                this.reportTime = registry.Timer("Metrics.Reporter." + name, Unit.Calls, SamplingType.FavourRecent, TimeUnit.Seconds, TimeUnit.Milliseconds);
            }
            this.reporter = reporter;
            this.registry = registry;
            this.interval = interval;
            this.scheduler = new ActionScheduler();
        }

        private void RunReport(CancellationToken token)
        {
            using (this.reportTime != null ? this.reportTime.NewContext() : null)
            {
                reporter().RunReport(this.registry, token);
            }
        }

        public void Start()
        {
            this.scheduler.Start(this.interval, RunReport);
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
