using System;
using System.Collections.Generic;
using Metrics.Meta;

namespace Metrics
{
    public interface MetricsRegistry
    {
        string Name { get; }

        IEnumerable<GaugeMeta> Gauges { get; }
        IEnumerable<CounterMeta> Counters { get; }
        IEnumerable<MeterMeta> Meters { get; }
        IEnumerable<HistogramMeta> Histograms { get; }
        IEnumerable<TimerMeta> Timers { get; }

        Gauge Gauge(string name, Func<string> valueProvider, Unit unit);
        Gauge Gauge(string name, Func<Gauge> gauge, Unit unit);
        Counter Counter(string name, Unit unit);
        Meter Meter(string name, Unit unit, TimeUnit rateUnit);
        Histogram Histogram(string name, Unit unit, SamplingType samplingType);
        Timer Timer(string name, Unit unit, SamplingType samplingType, TimeUnit rateUnit, TimeUnit durationUnit);

        void ClearAllMetrics();
    }
}
