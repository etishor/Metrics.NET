using System;
using System.Collections.Generic;
using Metrics.Core;

namespace Metrics.Tests.TestUtils
{
    public class TestRegistry : MetricsRegistry
    {
        public TestRegistry()
        { }

        public string Name { get { return "test"; } }

        public Timer TimerInstance { get; set; }
        public Meter MeterInstance { get; set; }
        public Histogram HistogramInstance { get; set; }
        public Counter CounterInstance { get; set; }
        public Gauge GaugeInstance { get; set; }

        public Func<string, Func<string>, Unit, Gauge> GaugeBuilder { get; set; }
        public Func<string, Unit, Counter> CounterBuilder { get; set; }
        public Func<string, Unit, TimeUnit, Meter> MeterBuilder { get; set; }
        public Func<string, Unit, SamplingType, Histogram> HistogramBuilder { get; set; }
        public Func<string, Unit, SamplingType, TimeUnit, TimeUnit, Timer> TimerBuilder { get; set; }

        public Gauge Gauge(string name, Func<string> valueProvider, Unit unit)
        {
            return GaugeInstance != null ? GaugeInstance : GaugeBuilder(name, valueProvider, unit);
        }

        public Gauge Gauge<T>(string name, Func<T> gauge, Unit unit) where T : GaugeMetric
        {
            return gauge();
        }

        public Counter Counter(string name, Unit unit)
        {
            return CounterInstance != null ? CounterInstance : CounterBuilder(name, unit);
        }

        public Meter Meter(string name, Unit unit, TimeUnit rateUnit)
        {
            return MeterInstance != null ? MeterInstance : MeterBuilder(name, unit, rateUnit);
        }

        public Histogram Histogram(string name, Unit unit, SamplingType samplingType)
        {
            return HistogramInstance != null ? HistogramInstance : HistogramBuilder(name, unit, samplingType);
        }

        public Timer Timer(string name, Unit unit, SamplingType samplingType, TimeUnit rateUnit, TimeUnit durationUnit)
        {
            return TimerInstance != null ? TimerInstance : TimerBuilder(name, unit, samplingType, rateUnit, durationUnit);
        }

        public IEnumerable<GaugeValueSource> Gauges { get { yield break; } }
        public IEnumerable<CounterValueSource> Counters { get { yield break; } }
        public IEnumerable<MeterValueSource> Meters { get { yield break; } }
        public IEnumerable<HistogramValueSource> Histograms { get { yield break; } }
        public IEnumerable<TimerValueSource> Timers { get { yield break; } }

        public void ClearAllMetrics() { }
    }
}
