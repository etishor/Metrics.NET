﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Metrics.MetricData;
using Metrics.Utils;

namespace Metrics.Reporters
{
    public abstract class BaseReport : MetricsReport
    {
        private CancellationToken token;

        public void RunReport(MetricsData metricsData, Func<HealthStatus> healthStatus, CancellationToken token)
        {
            this.token = token;

            this.Timestamp = Clock.Default.UTCDateTime;

            StartReport(metricsData.Context, metricsData.Timestamp);

            ReportContext(metricsData, Enumerable.Empty<string>());

            ReportHealthStatus(healthStatus, metricsData.Timestamp);

            EndReport(metricsData.Context, metricsData.Timestamp);
        }

        private void ReportContext(MetricsData data, IEnumerable<string> contextStack)
        {
            var contextName = FormatContextName(contextStack, data.Context);

            StartContext(contextName, data.Timestamp);

            ReportEnvironment(contextName, data.Environment);

            ReportSection("Gauges", data.Timestamp, data.Gauges, g => ReportGauge(FormatMetricName(contextName, g), g.Value, g.Unit, g.Tags));
            ReportSection("Counters", data.Timestamp, data.Counters, c => ReportCounter(FormatMetricName(contextName, c), c.Value, c.Unit, c.Tags));
            ReportSection("Meters", data.Timestamp, data.Meters, m => ReportMeter(FormatMetricName(contextName, m), m.Value, m.Unit, m.RateUnit, m.Tags));
            ReportSection("Histograms", data.Timestamp, data.Histograms, h => ReportHistogram(FormatMetricName(contextName, h), h.Value, h.Unit, h.Tags));
            ReportSection("Timers", data.Timestamp, data.Timers, t => ReportTimer(FormatMetricName(contextName, t), t.Value, t.Unit, t.RateUnit, t.DurationUnit, t.Tags));

            var stack = Enumerable.Concat(contextStack, new[] { data.Context });
            foreach (var child in data.ChildMetrics)
            {
                ReportContext(child, stack);
            }

            EndContext(contextName, data.Timestamp);
        }

        protected DateTime Timestamp { get; private set; }

        protected virtual void StartReport(string contextName, DateTime timestamp) { }
        protected virtual void StartContext(string contextName, DateTime timestamp) { }
        protected virtual void StartMetricGroup(string metricName, DateTime timestamp) { }
        protected virtual void EndMetricGroup(string metricName, DateTime timestamp) { }
        protected virtual void EndContext(string contextName, DateTime timestamp) { }
        protected virtual void EndReport(string contextName, DateTime timestamp) { }

        protected virtual void ReportEnvironment(string name, IEnumerable<EnvironmentEntry> environment) { }

        protected abstract void ReportGauge(string name, double value, Unit unit, MetricTags tags);
        protected abstract void ReportCounter(string name, CounterValue value, Unit unit, MetricTags tags);
        protected abstract void ReportMeter(string name, MeterValue value, Unit unit, TimeUnit rateUnit, MetricTags tags);
        protected abstract void ReportHistogram(string name, HistogramValue value, Unit unit, MetricTags tags);
        protected abstract void ReportTimer(string name, TimerValue value, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags);
        protected abstract void ReportHealth(HealthStatus status);

        protected virtual string FormatContextName(IEnumerable<string> contextStack, string contextName)
        {
            var stack = string.Join(" - ", contextStack);
            if (stack.Length == 0)
            {
                return contextName;
            }
            return string.Concat(stack, " - ", contextName);
        }

        protected virtual string FormatMetricName<T>(string context, MetricValueSource<T> metric)
        {
            return string.Concat("[", context, "] ", metric.Name);
        }

        private void ReportSection<T>(string name, DateTime timestamp, IEnumerable<T> metrics, Action<T> reporter)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            if (metrics.Any())
            {
                StartMetricGroup(name, timestamp);
                foreach (var metric in metrics)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    reporter(metric);
                }
                EndMetricGroup(name, timestamp);
            }
        }

        private void ReportHealthStatus(Func<HealthStatus> healthStatus, DateTime timestamp)
        {
            var status = healthStatus();
            if (!status.HasRegisteredChecks)
            {
                return;
            }
            StartMetricGroup("Health Checks", timestamp);
            ReportHealth(status);
        }
    }
}
