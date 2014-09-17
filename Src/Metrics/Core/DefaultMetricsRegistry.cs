using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Metrics.Core
{
    public sealed class DefaultMetricsRegistry : MetricsRegistry
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

        public DefaultMetricsRegistry()
        {
            this.DataProvider = new DefaultRegistryDataProvider(() => this.gauges.All, () => this.counters.All, () => this.meters.All, () => this.histograms.All, () => this.timers.All);
        }

        public RegistryDataProvider DataProvider { get; private set; }

        public void Gauge(string name, Func<MetricValueProvider<double>> valueProvider, Unit unit)
        {
            this.gauges.GetOrAdd(name, () =>
            {
                MetricValueProvider<double> gauge = valueProvider();
                return Tuple.Create(gauge, new GaugeValueSource(name, gauge, unit));
            });
        }

        public Counter Counter<T>(string name, Unit unit, Func<T> builder)
            where T : Counter, MetricValueProvider<long>
        {
            return this.counters.GetOrAdd(name, () =>
            {
                T counter = builder();
                return Tuple.Create((Counter)counter, new CounterValueSource(name, counter, unit));
            });
        }

        public Meter Meter<T>(string name, Unit unit, TimeUnit rateUnit, Func<T> builder)
            where T : Meter, MetricValueProvider<MeterValue>
        {
            return this.meters.GetOrAdd(name, () =>
            {
                T meter = builder();
                return Tuple.Create((Meter)meter, new MeterValueSource(name, meter, unit, rateUnit));
            });
        }

        public Histogram Histogram<T>(string name, Unit unit, Func<T> builder)
            where T : Histogram, MetricValueProvider<HistogramValue>
        {
            return this.histograms.GetOrAdd(name, () =>
            {
                T histogram = builder();
                return Tuple.Create((Histogram)histogram, new HistogramValueSource(name, histogram, unit));
            });
        }

        public Timer Timer<T>(string name, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, Func<T> builder)
            where T : Timer, MetricValueProvider<TimerValue>
        {
            return this.timers.GetOrAdd(name, () =>
            {
                T timer = builder();
                return Tuple.Create((Timer)timer, new TimerValueSource(name, timer, unit, rateUnit, durationUnit));
            });
        }

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
