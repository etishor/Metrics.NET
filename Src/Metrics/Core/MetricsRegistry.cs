
using System;
using System.Collections.Generic;
namespace Metrics.Core
{
    public interface RegistryDataProvider
    {
        IEnumerable<GaugeValueSource> Gauges { get; }
        IEnumerable<CounterValueSource> Counters { get; }
        IEnumerable<MeterValueSource> Meters { get; }
        IEnumerable<HistogramValueSource> Histograms { get; }
        IEnumerable<TimerValueSource> Timers { get; }
    }

    public interface MetricsRegistry
    {
        RegistryDataProvider DataProvider { get; }

        void Gauge(string name, Func<MetricValueProvider<double>> valueProvider, Unit unit, MetricTags tags);

        Counter Counter<T>(string name, Func<T> builder, Unit unit, MetricTags tags)
            where T : Counter, MetricValueProvider<long>;

        Meter Meter<T>(string name, Func<T> builder, Unit unit, TimeUnit rateUnit, MetricTags tags)
            where T : Meter, MetricValueProvider<MeterValue>;

        Histogram Histogram<T>(string name, Func<T> builder, Unit unit, MetricTags tags)
            where T : Histogram, MetricValueProvider<HistogramValue>;

        Timer Timer<T>(string name, Func<T> builder, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags)
            where T : Timer, MetricValueProvider<TimerValue>;

        void ClearAllMetrics();
        void ResetMetricsValues();
    }
}
