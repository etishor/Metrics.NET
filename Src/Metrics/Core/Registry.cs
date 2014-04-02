using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Metrics.Meta;

namespace Metrics.Core
{
    public class Registry : MetricsRegistry
    {
        private class MetricMetaCatalog<T, TMetric, TMetricValue>
            where T : MetricMeta<TMetric, TMetricValue>
            where TMetric : Metric<TMetricValue>
            where TMetricValue : struct
        {
            private readonly ConcurrentDictionary<string, T> metrics = new ConcurrentDictionary<string, T>();

            public IEnumerable<T> All { get { return this.metrics.Values.OrderBy(m => m.Name).AsEnumerable(); } }

            public TMetric GetOrAdd(string name, Func<T> metricProvider)
            {
                return this.metrics.GetOrAdd(name, n => metricProvider()).Metric();
            }

            public void Clear()
            {
                this.metrics.Clear();
            }
        }

        private readonly MetricMetaCatalog<GaugeMeta, Gauge, GaugeValue> gauges = new MetricMetaCatalog<GaugeMeta, Gauge, GaugeValue>();
        private readonly MetricMetaCatalog<CounterMeta, Counter, long> counters = new MetricMetaCatalog<CounterMeta, Counter, long>();
        private readonly MetricMetaCatalog<MeterMeta, Meter, MeterValue> meters = new MetricMetaCatalog<MeterMeta, Meter, MeterValue>();
        private readonly MetricMetaCatalog<HistogramMeta, Histogram, HistogramValue> histograms = new MetricMetaCatalog<HistogramMeta, Histogram, HistogramValue>();
        private readonly MetricMetaCatalog<TimerMeta, Timer, TimerValue> timers = new MetricMetaCatalog<TimerMeta, Timer, TimerValue>();

        public Registry()
            : this(Process.GetCurrentProcess().ProcessName) { }

        public Registry(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }

        public IEnumerable<GaugeMeta> Gauges { get { return this.gauges.All; } }
        public IEnumerable<CounterMeta> Counters { get { return this.counters.All; } }
        public IEnumerable<MeterMeta> Meters { get { return this.meters.All; } }
        public IEnumerable<HistogramMeta> Histograms { get { return this.histograms.All; } }
        public IEnumerable<TimerMeta> Timers { get { return this.timers.All; } }

        public Gauge Gauge(string name, Func<string> valueProvider, Unit unit)
        {
            return this.gauges.GetOrAdd(name, () => new GaugeMeta(name, new GaugeMetric(valueProvider), unit));
        }

        public Gauge Gauge(string name, Func<Gauge> gauge, Unit unit)
        {
            return this.gauges.GetOrAdd(name, () => new GaugeMeta(name, gauge(), unit));
        }

        public Meter Meter(string name, Unit unit, TimeUnit rateUnit)
        {
            return this.meters.GetOrAdd(name, () => new MeterMeta(name, new MeterMetric(), unit, rateUnit));
        }

        public Counter Counter(string name, Unit unit)
        {
            return this.counters.GetOrAdd(name, () => new CounterMeta(name, new CounterMetric(), unit));
        }

        public Histogram Histogram(string name, Unit unit, SamplingType samplingType)
        {
            return this.histograms.GetOrAdd(name, () => new HistogramMeta(name, new HistogramMetric(samplingType), unit));
        }

        public Timer Timer(string name, Unit unit, SamplingType samplingType, TimeUnit rateUnit, TimeUnit durationUnit)
        {
            return this.timers.GetOrAdd(name, () => new TimerMeta(name, new TimerMetric(samplingType), unit, rateUnit, durationUnit));
        }

        public void ClearAllMetrics()
        {
            this.gauges.Clear();
            this.counters.Clear();
            this.meters.Clear();
            this.histograms.Clear();
            this.timers.Clear();
        }
    }
}
