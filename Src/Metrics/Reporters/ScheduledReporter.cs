using System;

namespace Metrics.Reporters
{
    public class ScheduledReporter
    {
        private readonly Timer reportTime = null;

        private readonly Func<Reporter> reporter;
        private readonly TimeSpan interval;
        private readonly MetricsRegistry registry;
        private System.Threading.Timer timer;

        public ScheduledReporter(string name, Func<Reporter> reporter, MetricsRegistry registry, TimeSpan interval)
        {
            if (Metric.Reports.EnableReportDiagnosticMetrics)
            {
                this.reportTime = registry.Timer("Metrics.Reporter." + name, Unit.Calls, SamplingType.FavourRecent, TimeUnit.Seconds, TimeUnit.Milliseconds);
            }
            this.reporter = reporter;
            this.interval = interval;
            this.registry = registry;
        }

        public void Start()
        {
            if (this.timer != null)
            {
                Stop();
            }
            this.timer = new System.Threading.Timer((s) => RunReport(), null, interval, interval);
        }

        private void RunReport()
        {
            try
            {
                using (this.reportTime != null ? this.reportTime.NewContext() : null)
                using (var report = reporter())
                {
                    report.RunReport(this.registry);
                }
            }
            catch (Exception x)
            {
                if (Metric.ErrorHandler != null)
                {
                    Metric.ErrorHandler(x);
                }
                else
                {
                    throw;
                }
            }
        }

        public void Stop()
        {
            this.timer.Dispose();
            this.timer = null;
        }
    }
}
