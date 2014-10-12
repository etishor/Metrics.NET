using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Metrics.MetricData;
using Metrics.Utils;

namespace Metrics.Reporters
{
    public abstract class BaseReporter : MetricsReporter
    {
        private CancellationToken token;

        public void RunReport(MetricsData metricsData, Func<HealthStatus> healthStatus, CancellationToken token)
        {
            this.token = token;
            this.Timestamp = Clock.Default.UTCDateTime;

            var data = metricsData.OldFormat();

            this.Context = metricsData.Context;

            StartReport();
            ReportSection("Gauges", data.Gauges, g => ReportGauge(g.Name, g.Value, g.Unit));
            ReportSection("Counters", data.Counters, c => ReportCounter(c.Name, c.Value, c.Unit));
            ReportSection("Meters", data.Meters, m => ReportMeter(m.Name, m.Value, m.Unit, m.RateUnit));
            ReportSection("Histograms", data.Histograms, h => ReportHistogram(h.Name, h.Value, h.Unit));
            ReportSection("Timers", data.Timers, t => ReportTimer(t.Name, t.Value, t.Unit, t.RateUnit, t.DurationUnit));
            ReportHealthStatus(healthStatus);
            EndReport();
        }

        protected string Context { get; private set; }
        protected DateTime Timestamp { get; private set; }

        protected virtual void StartReport() { }
        protected virtual void StartMetricGroup(string metricName) { }
        protected virtual void EndMetricGroup(string metricName) { }
        protected virtual void EndReport() { }

        protected abstract void ReportGauge(string name, double value, Unit unit);
        protected abstract void ReportCounter(string name, CounterValue value, Unit unit);
        protected abstract void ReportMeter(string name, MeterValue value, Unit unit, TimeUnit rateUnit);
        protected abstract void ReportHistogram(string name, HistogramValue value, Unit unit);
        protected abstract void ReportTimer(string name, TimerValue value, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit);
        protected abstract void ReportHealth(HealthStatus status);

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

        private void ReportHealthStatus(Func<HealthStatus> healthStatus)
        {
            var status = healthStatus();
            if (!status.HasRegisteredChecks)
            {
                return;
            }
            StartMetricGroup("Health Checks");
            ReportHealth(status);
        }

    }
}
