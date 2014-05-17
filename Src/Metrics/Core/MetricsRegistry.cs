using System;
using System.Collections.Generic;
using Metrics.Core;

namespace Metrics.Core
{
    public interface MetricsRegistry
    {
        string Name { get; }

        IEnumerable<GaugeValueSource> Gauges { get; }
        IEnumerable<CounterValueSource> Counters { get; }
        IEnumerable<MeterValueSource> Meters { get; }
        IEnumerable<HistogramValueSource> Histograms { get; }
        IEnumerable<TimerValueSource> Timers { get; }

        Gauge Gauge(string name, Func<double> valueProvider, Unit unit);
        Gauge Gauge<T>(string name, Func<T> gauge, Unit unit) where T : GaugeMetric;
        Counter Counter(string name, Unit unit);
        Meter Meter(string name, Unit unit, TimeUnit rateUnit);
        Histogram Histogram(string name, Unit unit, SamplingType samplingType);
        Timer Timer(string name, Unit unit, SamplingType samplingType, TimeUnit rateUnit, TimeUnit durationUnit);

        void ClearAllMetrics();
    }
}
