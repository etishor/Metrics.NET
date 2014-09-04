
using System;
namespace Metrics.Core
{
    public interface MetricsRegistry
    {
        MetricsData MetricsData { get; }

        void Gauge(string name, Func<MetricValueProvider<double>> valueProvider, Unit unit);

        Counter Counter(string name, Unit unit);
        Meter Meter(string name, Unit unit, TimeUnit rateUnit);
        Histogram Histogram(string name, Unit unit, SamplingType samplingType);
        Timer Timer(string name, Unit unit, SamplingType samplingType, TimeUnit rateUnit, TimeUnit durationUnit);

        void ClearAllMetrics();
    }
}
