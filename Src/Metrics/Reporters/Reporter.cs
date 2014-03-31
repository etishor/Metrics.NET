using System;
using System.Collections.Generic;
using System.Linq;

namespace Metrics.Reporters
{
    public abstract class Reporter : IDisposable, Utils.IHideObjectMembers
    {
        public void RunReport(MetricsRegistry registry)
        {
            this.Timestamp = DateTime.Now;
            this.RegistryName = registry.Name;
            StartReport();
            ReportSection("Gauges", registry.Gauges, g => ReportGauge(g.Name, g.Value.Value, g.Unit));
            ReportSection("Counters", registry.Counters, c => ReportCounter(c.Name, c.Value, c.Unit));
            ReportSection("Meters", registry.Meters, m => ReportMeter(m.Name, m.Value, m.Unit, m.RateUnit));
            ReportSection("Histograms", registry.Histograms, h => ReportHistogram(h.Name, h.Value, h.Unit));
            ReportSection("Timers", registry.Timers, t => ReportTimer(t.Name, t.Value, t.Unit, t.RateUnit, t.DurationUnit));
            EndReport();
        }

        protected string RegistryName { get; private set; }
        protected DateTime Timestamp { get; private set; }

        protected virtual void StartReport() { }
        protected virtual void StartMetricGroup(string metricName) { }
        protected virtual void EndMetricGroup(string metricName) { }
        protected virtual void EndReport() { }

        protected abstract void ReportGauge(string name, string value, Unit unit);
        protected abstract void ReportCounter(string name, long value, Unit unit);
        protected abstract void ReportMeter(string name, MeterValue value, Unit unit, TimeUnit rateUnit);
        protected abstract void ReportHistogram(string name, HistogramValue value, Unit unit);
        protected abstract void ReportTimer(string name, TimerValue value, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit);

        private void ReportSection<T>(string name, IEnumerable<T> metrics, Action<T> reporter)
        {
            if (metrics.Any())
            {
                StartMetricGroup(name);
                foreach (var metric in metrics)
                {
                    reporter(metric);
                }
                EndMetricGroup(name);
            }
        }

        private bool disposed;

        ~Reporter()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            if (!this.disposed)
            {
                Dispose(true);
            }
            this.disposed = true;
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        { }
    }
}
