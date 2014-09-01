using System;
namespace Metrics
{
    public interface MetricsContext : IDisposable
    {
        MetricsData MetricsData { get; }

        MetricsConfig Config { get; }

        MetricsContext Context(string contextName);
        MetricsContext Context(string contextName, Func<string, MetricsContext> contextCreator);
        void ShutdownContext(string contextName);

        Gauge Gauge(string name, Func<double> valueProvider, Unit unit);
        Gauge PerformanceCounter(string name, string counterCategory, string counterName, string counterInstance, Unit unit);
        Counter Counter(string name, Unit unit);
        Meter Meter(string name, Unit unit, TimeUnit rateUnit = TimeUnit.Seconds);
        Histogram Histogram(string name, Unit unit, SamplingType samplingType = SamplingType.FavourRecent);
        Timer Timer(string name, Unit unit, SamplingType samplingType = SamplingType.FavourRecent, TimeUnit rateUnit = TimeUnit.Seconds, TimeUnit durationUnit = TimeUnit.Milliseconds);

        void CompletelyDisableMetrics();
    }
}
