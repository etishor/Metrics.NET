using System;
using System.Collections.Generic;

namespace Metrics.Tests.TestUtils
{
    public class TestRegistry : MetricsRegistry
    {
        public TestRegistry()
        {

        }

        public TestRegistry(Timer timer)
        {
            this.TimerInstance = timer;
        }

        public string Name { get { return "test"; } }

        public Timer TimerInstance { get; set; }

        public Func<string, Func<string>, Unit, Gauge> GaugeBuilder { get; set; }
        public Func<string, Unit, Counter> CounterBuilder { get; set; }
        public Func<string, Unit, TimeUnit, Meter> MeterBuilder { get; set; }
        public Func<string, Unit, SamplingType, Histogram> HistogramBuilder { get; set; }
        public Func<string, Unit, SamplingType, TimeUnit, TimeUnit, Timer> TimerBuilder { get; set; }

        public Gauge Gauge(string name, Func<string> valueProvider, Unit unit)
        {
            return GaugeBuilder(name, valueProvider, unit);
        }

        public Gauge Gauge(string name, Func<Gauge> gauge, Unit unit)
        {
            return gauge();
        }

        public Counter Counter(string name, Unit unit)
        {
            return CounterBuilder(name, unit);
        }

        public Meter Meter(string name, Unit unit, TimeUnit rateUnit)
        {
            return MeterBuilder(name, unit, rateUnit);
        }

        public Histogram Histogram(string name, Unit unit, SamplingType samplingType)
        {
            return HistogramBuilder(name, unit, samplingType);
        }

        public Timer Timer(string name, Unit unit, SamplingType samplingType, TimeUnit rateUnit, TimeUnit durationUnit)
        {
            return TimerInstance != null ? TimerInstance : TimerBuilder(name, unit, samplingType, rateUnit, durationUnit);
        }

        public IEnumerable<Meta.GaugeMeta> Gauges
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<Meta.CounterMeta> Counters
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<Meta.MeterMeta> Meters
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<Meta.HistogramMeta> Histograms
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<Meta.TimerMeta> Timers
        {
            get { throw new NotImplementedException(); }
        }


        public void ClearAllMetrics()
        {
            throw new NotImplementedException();
        }
    }
}
