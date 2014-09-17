using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Metrics.Core
{
    /// <summary>
    /// Encapsulates common functionality for a metrics registry
    /// </summary>
    public abstract class BaseRegistry : MetricsRegistry
    {
        private class MetricMetaCatalog<TMetric, TValue, TMetricValue>
            where TValue : MetricValueSource<TMetricValue>
            where TMetricValue : struct
        {
            public class MetricMeta
            {
                public MetricMeta(TMetric metric, TValue valueUnit)
                {
                    this.Metric = metric;
                    this.Value = valueUnit;
                }

                public string Name { get { return this.Value.Name; } }
                public TMetric Metric { get; private set; }
                public TValue Value { get; private set; }
            }

            private readonly ConcurrentDictionary<string, MetricMeta> metrics =
                new ConcurrentDictionary<string, MetricMeta>();

            public IEnumerable<TValue> All
            {
                get
                {
                    return this.metrics.Values.OrderBy(m => m.Name).Select(v => v.Value);
                }
            }

            public TMetric GetOrAdd(string name, Func<Tuple<TMetric, TValue>> metricProvider)
            {
                return this.metrics.GetOrAdd(name, n =>
                {
                    var result = metricProvider();
                    return new MetricMeta(result.Item1, result.Item2);
                }).Metric;
            }

            public void Clear()
            {
                var values = this.metrics.Values;
                this.metrics.Clear();
                foreach (var value in values)
                {
                    using (value.Metric as IDisposable) { }
                }
            }

            public void Reset()
            {
                foreach (var metric in this.metrics.Values)
                {
                    var resetable = metric.Metric as ResetableMetric;
                    if (resetable != null)
                    {
                        resetable.Reset();
                    }
                }
            }
        }

        private readonly MetricMetaCatalog<MetricValueProvider<double>, GaugeValueSource, double> gauges = new MetricMetaCatalog<MetricValueProvider<double>, GaugeValueSource, double>();
        private readonly MetricMetaCatalog<Counter, CounterValueSource, long> counters = new MetricMetaCatalog<Counter, CounterValueSource, long>();
        private readonly MetricMetaCatalog<Meter, MeterValueSource, MeterValue> meters = new MetricMetaCatalog<Meter, MeterValueSource, MeterValue>();
        private readonly MetricMetaCatalog<Histogram, HistogramValueSource, HistogramValue> histograms =
            new MetricMetaCatalog<Histogram, HistogramValueSource, HistogramValue>();
        private readonly MetricMetaCatalog<Timer, TimerValueSource, TimerValue> timers = new MetricMetaCatalog<Timer, TimerValueSource, TimerValue>();

        public BaseRegistry(string name)
        {
            this.Name = name;
            this.DataProvider = new DefaultRegistryDataProvider(() => this.gauges.All, () => this.counters.All, () => this.meters.All, () => this.histograms.All, () => this.timers.All);
        }

        public string Name { get; private set; }
        public RegistryDataProvider DataProvider { get; private set; }

        public void Gauge(string name, Func<MetricValueProvider<double>> valueProvider, Unit unit)
        {
            this.gauges.GetOrAdd(name, () => CreateGauge(name, valueProvider, unit));
        }

        public Counter Counter(string name, Unit unit)
        {
            return this.counters.GetOrAdd(name, () => CreateCounter(name, unit));
        }

        public Meter Meter(string name, Unit unit, TimeUnit rateUnit)
        {
            return this.meters.GetOrAdd(name, () => CreateMeter(name, unit, rateUnit));
        }

        public Histogram Histogram(string name, Unit unit, SamplingType samplingType)
        {
            return this.histograms.GetOrAdd(name, () => CreateHistogram(name, unit, samplingType));
        }

        public Timer Timer(string name, Unit unit, SamplingType samplingType, TimeUnit rateUnit, TimeUnit durationUnit)
        {
            return this.timers.GetOrAdd(name, () => CreateTimer(name, unit, samplingType, rateUnit, durationUnit));
        }

        protected abstract Tuple<MetricValueProvider<double>, GaugeValueSource> CreateGauge(string name, Func<MetricValueProvider<double>> valueProvider, Unit unit);
        protected abstract Tuple<Counter, CounterValueSource> CreateCounter(string name, Unit unit);
        protected abstract Tuple<Meter, MeterValueSource> CreateMeter(string name, Unit unit, TimeUnit rateUnit);
        protected abstract Tuple<Histogram, HistogramValueSource> CreateHistogram(string name, Unit unit, SamplingType samplingType);
        protected abstract Tuple<Timer, TimerValueSource> CreateTimer(string name, Unit unit, SamplingType samplingType, TimeUnit rateUnit, TimeUnit durationUnit);

        public void ClearAllMetrics()
        {
            this.gauges.Clear();
            this.counters.Clear();
            this.meters.Clear();
            this.histograms.Clear();
            this.timers.Clear();
        }

        public void ResetMetricsValues()
        {
            this.gauges.Reset();
            this.counters.Reset();
            this.meters.Reset();
            this.histograms.Reset();
            this.timers.Reset();
        }
    }
}
