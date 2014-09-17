
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

        void Gauge(string name, Func<MetricValueProvider<double>> valueProvider, Unit unit);
        Counter Counter(string name, Unit unit);
        Meter Meter(string name, Unit unit, TimeUnit rateUnit);
        Histogram Histogram(string name, Unit unit, SamplingType samplingType);
        Timer Timer(string name, Unit unit, SamplingType samplingType, TimeUnit rateUnit, TimeUnit durationUnit);

        void ClearAllMetrics();
        void ResetMetricsValues();
    }
}
