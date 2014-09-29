using System;
using System.Collections.Generic;

namespace Metrics.Core
{
    public sealed class NullMetricsRegistry : MetricsRegistry
    {
        private class NullCounter : Counter
        {
            public static readonly Counter Instance = new NullCounter();
            public void Increment() { }
            public void Increment(long value) { }
            public void Decrement() { }
            public void Decrement(long value) { }
            public void Increment(string item) { }
            public void Increment(string item, long value) { }
            public void Decrement(string item) { }
            public void Decrement(string item, long value) { }
            public void Reset() { }
        }

        private class NullMeter : Meter
        {
            public static readonly Meter Instance = new NullMeter();
            public void Mark() { }
            public void Mark(long count) { }
            public void Mark(string item) { }
            public void Mark(string item, long count) { }
            public void Reset() { }
        }

        private class NullHistogram : Histogram
        {
            public static readonly Histogram Instance = new NullHistogram();
            public void Update(long value, string userValue) { }
            public void Reset() { }
        }

        private class NullTimer : Timer
        {
            public static readonly Timer Instance = new NullTimer();
            public void Record(long time, TimeUnit unit, string userValue = null) { }
            public void Time(Action action, string userValue = null) { action(); }
            public T Time<T>(Func<T> action, string userValue = null) { return action(); }
            public TimerContext NewContext(string userValue = null) { return null; }
            public TimerContext NewContext(Action<TimeSpan> finalAction, string userValue = null) { return null; }
            public void Reset() { }
        }

        private class NullDataProvider : RegistryDataProvider
        {
            public static readonly NullDataProvider Instance = new NullDataProvider();
            public IEnumerable<GaugeValueSource> Gauges { get { yield break; } }
            public IEnumerable<CounterValueSource> Counters { get { yield break; } }
            public IEnumerable<MeterValueSource> Meters { get { yield break; } }
            public IEnumerable<HistogramValueSource> Histograms { get { yield break; } }
            public IEnumerable<TimerValueSource> Timers { get { yield break; } }
        }

        public RegistryDataProvider DataProvider { get { return NullDataProvider.Instance; } }

        public void ClearAllMetrics() { }
        public void ResetMetricsValues() { }

        public void Gauge(string name, Func<MetricValueProvider<double>> valueProvider, Unit unit, MetricTags tags) { }

        public Counter Counter<T>(string name, Func<T> builder, Unit unit, MetricTags tags) where T : CounterImplementation
        {
            return NullCounter.Instance;
        }

        public Meter Meter<T>(string name, Func<T> builder, Unit unit, TimeUnit rateUnit, MetricTags tags) where T : MeterImplementation
        {
            return NullMeter.Instance;
        }

        public Histogram Histogram<T>(string name, Func<T> builder, Unit unit, MetricTags tags) where T : HistogramImplementation
        {
            return NullHistogram.Instance;
        }

        public Timer Timer<T>(string name, Func<T> builder, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags) where T : TimerImplementation
        {
            return NullTimer.Instance;
        }
    }
}
