using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Metrics.Utils;

namespace Metrics.Reporters
{
    public abstract class Reporter : Utils.IHideObjectMembers
    {
        private CancellationToken token;

        public void RunReport(MetricsRegistry registry, HealthChecksRegistry healthChecks)
        {
            RunReport(registry, healthChecks, CancellationToken.None);
        }

        public void RunReport(MetricsRegistry registry, HealthChecksRegistry healthChecks, CancellationToken token)
        {
            this.token = token;
            this.Timestamp = Clock.Default.LocalDateTime;
            this.RegistryName = registry.Name;

            StartReport();
            ReportSection("Gauges", registry.Gauges, g => ReportGauge(g.Name, g.Value, g.Unit));
            ReportSection("Counters", registry.Counters, c => ReportCounter(c.Name, c.Value, c.Unit));
            ReportSection("Meters", registry.Meters, m => ReportMeter(m.Name, m.Value, m.Unit, m.RateUnit));
            ReportSection("Histograms", registry.Histograms, h => ReportHistogram(h.Name, h.Value, h.Unit));
            ReportSection("Timers", registry.Timers, t => ReportTimer(t.Name, t.Value, t.Unit, t.RateUnit, t.DurationUnit));
            ReportHealthStatus(healthChecks);
            EndReport();
        }

        protected string RegistryName { get; private set; }
        protected DateTime Timestamp { get; private set; }

        protected virtual void StartReport() { }
        protected virtual void StartMetricGroup(string metricName) { }
        protected virtual void EndMetricGroup(string metricName) { }
        protected virtual void EndReport() { }

        protected abstract void ReportGauge(string name, double value, Unit unit);
        protected abstract void ReportCounter(string name, long value, Unit unit);
        protected abstract void ReportMeter(string name, MeterValue value, Unit unit, TimeUnit rateUnit);
        protected abstract void ReportHistogram(string name, HistogramValue value, Unit unit);
        protected abstract void ReportTimer(string name, TimerValue value, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit);
        protected abstract void ReportHealth(string name, HealthStatus status);

        private void ReportSection<T>(string name, IEnumerable<T> metrics, Action<T> reporter)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            if (metrics.Any())
            {
                StartMetricGroup(name);
                foreach (var metric in metrics)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    reporter(metric);
                }
                EndMetricGroup(name);
            }
        }

        private void ReportHealthStatus(HealthChecksRegistry healthChecks)
        {
            var status = healthChecks.GetStatus();
            if (!status.HasRegisteredChecks)
            {
                return;
            }
            StartMetricGroup("Health Checks");
            ReportHealth(healthChecks.Name, status);
        }

    }
}
