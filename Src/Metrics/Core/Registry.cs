using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Metrics.Meta;

namespace Metrics.Core
{
    public sealed class Registry : MetricsRegistry
    {
        private class MetricMetaCatalog<TMeta, TMetricImpl, TMetric, TValue>
            where TMeta : MetricMeta<TMetricImpl, TMetric, TValue>
            where TMetricImpl : TMetric, MetricValue<TValue>
            where TValue : struct
        {
            private readonly ConcurrentDictionary<string, TMeta> metrics = new ConcurrentDictionary<string, TMeta>();

            public IEnumerable<TMeta> All { get { return this.metrics.Values.OrderBy(m => m.Name).AsEnumerable(); } }

            public TMetric GetOrAdd(string name, Func<TMeta> metricProvider)
            {
                return this.metrics.GetOrAdd(name, n => metricProvider()).Metric();
            }

            public void Clear()
            {
                this.metrics.Clear();
            }
        }

        private readonly MetricMetaCatalog<GaugeMeta, GaugeMetric, Gauge, GaugeValue> gauges = new MetricMetaCatalog<GaugeMeta, GaugeMetric, Gauge, GaugeValue>();
        private readonly MetricMetaCatalog<CounterMeta, CounterMetric, Counter, long> counters = new MetricMetaCatalog<CounterMeta, CounterMetric, Counter, long>();
        private readonly MetricMetaCatalog<MeterMeta, MeterMetric, Meter, MeterValue> meters = new MetricMetaCatalog<MeterMeta, MeterMetric, Meter, MeterValue>();
        private readonly MetricMetaCatalog<HistogramMeta, HistogramMetric, Histogram, HistogramValue> histograms = new MetricMetaCatalog<HistogramMeta, HistogramMetric, Histogram, HistogramValue>();
        private readonly MetricMetaCatalog<TimerMeta, TimerMetric, Timer, TimerValue> timers = new MetricMetaCatalog<TimerMeta, TimerMetric, Timer, TimerValue>();

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
            return this.gauges.GetOrAdd(name, () => new GaugeMeta(name, new SimpleGauge(valueProvider), unit));
        }

        public Gauge Gauge<T>(string name, Func<T> gauge, Unit unit)
             where T : GaugeMetric
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
