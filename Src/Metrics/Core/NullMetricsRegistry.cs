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
            public void Reset() { }
        }

        private class NullMeter : Meter
        {
            public static readonly Meter Instance = new NullMeter();
            public void Mark() { }
            public void Mark(long count) { }
            public void Reset() { }
        }

        private class NullHistogram : Histogram
        {
            public static readonly Histogram Instance = new NullHistogram();
            public void Update(long value) { }
            public void Reset() { }
        }

        private class NullTimer : Timer
        {
            public static readonly Timer Instance = new NullTimer();
            public void Record(long time, TimeUnit unit) { }
            public void Time(Action action) { action(); }
            public T Time<T>(Func<T> action) { return action(); }
            public TimerContext NewContext() { return null; }
            public TimerContext NewContext(Action<TimeSpan> finalAction) { return null; }
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

        public Counter Counter<T>(string name, Func<T> builder, Unit unit, MetricTags tags) where T : Counter, MetricValueProvider<long>
        {
            return NullCounter.Instance;
        }

        public Meter Meter<T>(string name, Func<T> builder, Unit unit, TimeUnit rateUnit, MetricTags tags) where T : Meter, MetricValueProvider<MeterValue>
        {
            return NullMeter.Instance;
        }

        public Histogram Histogram<T>(string name, Func<T> builder, Unit unit, MetricTags tags) where T : Histogram, MetricValueProvider<HistogramValue>
        {
            return NullHistogram.Instance;
        }

        public Timer Timer<T>(string name, Func<T> builder, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags) where T : Timer, MetricValueProvider<TimerValue>
        {
            return NullTimer.Instance;
        }
    }
}
