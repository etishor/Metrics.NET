using System;

namespace Metrics.Reporters
{
    public class ScheduledReporter
    {
        private readonly Timer reportTime;

        private readonly Func<Reporter> reporter;
        private readonly TimeSpan interval;
        private readonly MetricsRegistry registry;
        private System.Threading.Timer timer;

        public ScheduledReporter(string name, Func<Reporter> reporter, MetricsRegistry registry, TimeSpan interval)
        {
            this.reportTime = Metric.Timer("Metrics.Reporter." + name, Unit.Calls);
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
            using (this.reportTime.NewContext())
            using (var report = reporter())
            {
                report.RunReport(this.registry);
            }
        }

        public void Stop()
        {
            this.timer.Dispose();
            this.timer = null;
        }
    }
}
