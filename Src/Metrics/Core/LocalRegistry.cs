using System;
using System.Diagnostics;

namespace Metrics.Core
{
    public sealed class LocalRegistry : BaseRegistry
    {
        public LocalRegistry()
            : base(Process.GetCurrentProcess().ProcessName) { }

        protected override Tuple<Gauge, GaugeValueSource> CreateGauge(string name, Func<double> valueProvider, Unit unit)
        {
            var gauge = new FunctionGauge(valueProvider);
            return Tuple.Create((Gauge)gauge, new GaugeValueSource(name, gauge, unit));
        }

        protected override Tuple<Gauge, GaugeValueSource> CreateGauge<T>(string name, Func<T> gauge, Unit unit)
        {
            var gaugeMetric = gauge();
            return Tuple.Create((Gauge)gaugeMetric, new GaugeValueSource(name, gaugeMetric, unit));
        }

        protected override Tuple<Counter, CounterValueSource> CreateCounter(string name, Unit unit)
        {
            var counter = new CounterMetric();
            return Tuple.Create((Counter)counter, new CounterValueSource(name, counter, unit));
        }

        protected override Tuple<Meter, MeterValueSource> CreateMeter(string name, Unit unit, TimeUnit rateUnit)
        {
            var meter = new MeterMetric();
            return Tuple.Create((Meter)meter, new MeterValueSource(name, meter, unit, rateUnit));
        }

        protected override Tuple<Histogram, HistogramValueSource> CreateHistogram(string name, Unit unit, SamplingType samplingType)
        {
            var histogram = new HistogramMetric(samplingType);
            return Tuple.Create((Histogram)histogram, new HistogramValueSource(name, histogram, unit));
        }

        protected override Tuple<Timer, TimerValueSource> CreateTimer(string name, Unit unit, SamplingType samplingType, TimeUnit rateUnit, TimeUnit durationUnit)
        {
            var timer = new TimerMetric(samplingType);
            return Tuple.Create((Timer)timer, new TimerValueSource(name, timer, unit, rateUnit, durationUnit));
        }
    }
}
