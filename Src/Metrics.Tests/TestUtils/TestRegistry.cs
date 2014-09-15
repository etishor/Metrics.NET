using System;
using Metrics.Core;
using Metrics.Utils;

namespace Metrics.Tests.TestUtils
{
    public class TestRegistry : BaseRegistry
    {
        private readonly Clock clock;
        private readonly Scheduler scheduler;

        public TestRegistry(string registryName, Clock clock, Scheduler scheduler)
            : base(registryName)
        {
            this.clock = clock;
            this.scheduler = scheduler;
        }

        protected override Tuple<MetricValueProvider<double>, GaugeValueSource> CreateGauge(string name, Func<MetricValueProvider<double>> valueProvider, Unit unit)
        {
            var provider = valueProvider();
            return Tuple.Create(provider, new GaugeValueSource(name, provider, unit));
        }

        protected override Tuple<Counter, CounterValueSource> CreateCounter(string name, Unit unit)
        {

            var counter = new CounterMetric();
            return Tuple.Create((Counter)counter, new CounterValueSource(name, counter, unit));
        }

        protected override Tuple<Meter, MeterValueSource> CreateMeter(string name, Unit unit, TimeUnit rateUnit)
        {
            var meter = new MeterMetric(this.clock, this.scheduler);
            return Tuple.Create((Meter)meter, new MeterValueSource(name, meter, unit, rateUnit));
        }

        protected override Tuple<Histogram, HistogramValueSource> CreateHistogram(string name, Unit unit, SamplingType samplingType)
        {
            var histogram = new HistogramMetric(new SlidingWindowReservoir()); // always use sliding window reservoir as we test with less than 1028 values
            return Tuple.Create((Histogram)histogram, new HistogramValueSource(name, histogram, unit));
        }

        protected override Tuple<Timer, TimerValueSource> CreateTimer(string name, Unit unit, SamplingType samplingType, TimeUnit rateUnit, TimeUnit durationUnit)
        {
            // always use sliding window reservoir as we test with less than 1028 values
            var timer = new TimerMetric(new HistogramMetric(new SlidingWindowReservoir()), new MeterMetric(this.clock, this.scheduler), this.clock);
            return Tuple.Create((Timer)timer, new TimerValueSource(name, timer, unit, rateUnit, durationUnit));
        }
    }
}
