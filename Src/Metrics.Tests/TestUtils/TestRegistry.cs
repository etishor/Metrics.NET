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

        //public new FunctionGauge FindGauge(string name) { return base.FindGauge(name) as FunctionGauge; }
        //public new CounterMetric FindCounter(string name) { return base.FindCounter(name) as CounterMetric; }
        //public new MeterMetric FindMeter(string name) { return base.FindMeter(name) as MeterMetric; }
        //public new HistogramMetric FindHistogram(string name) { return base.FindHistogram(name) as HistogramMetric; }
        //public new TimerMetric FindTimer(string name) { return base.FindTimer(name) as TimerMetric; }

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
            var meter = new MeterMetric(this.clock, this.scheduler);
            return Tuple.Create((Meter)meter, new MeterValueSource(name, meter, unit, rateUnit));
        }

        protected override Tuple<Histogram, HistogramValueSource> CreateHistogram(string name, Unit unit, SamplingType samplingType)
        {
            var histogram = new HistogramMetric(SamplingTypeToReservoir(samplingType));
            return Tuple.Create((Histogram)histogram, new HistogramValueSource(name, histogram, unit));
        }

        protected override Tuple<Timer, TimerValueSource> CreateTimer(string name, Unit unit, SamplingType samplingType, TimeUnit rateUnit, TimeUnit durationUnit)
        {
            var timer = new TimerMetric(new HistogramMetric(SamplingTypeToReservoir(samplingType)), new MeterMetric(this.clock, this.scheduler), this.clock);
            return Tuple.Create((Timer)timer, new TimerValueSource(name, timer, unit, rateUnit, durationUnit));
        }

        private Reservoir SamplingTypeToReservoir(SamplingType samplingType)
        {
            return new SlidingWindowReservoir();
            //switch (samplingType)
            //{
            //    case SamplingType.FavourRecent: return new ExponentiallyDecayingReservoir(this.clock, this.scheduler);
            //    case SamplingType.LongTerm: return new UniformReservoir();
            //    case SamplingType.SlidingWindow: return new SlidingWindowReservoir();
            //}
            //throw new System.NotImplementedException();
        }
    }
}
