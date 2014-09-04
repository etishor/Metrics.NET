
namespace Metrics
{
    public struct GaugeData
    {
        public readonly string Name;
        public readonly double Value;
        public readonly Unit Unit;
    }

    public struct CounterData
    {
        public readonly string Name;
        public readonly double Value;
        public readonly Unit Unit;
    }

    public struct MeterData
    {
        public readonly string Name;
        public readonly MeterValue Value;
        public readonly Unit Unit;
        public readonly TimeUnit RateUnit;
    }

    public struct HistogramData
    {
        public readonly string Name;
        public readonly HistogramValue Value;
        public readonly Unit Unit;
    }

    public struct TimerData
    {
        public readonly string Name;
        public readonly TimerValue Value;
        public readonly Unit Unit;
        public TimeUnit RateUnit { get; private set; }
        public TimeUnit DurationUnit { get; private set; }
    }



    /// <summary>
    /// A provider capable of returning the current values for a set of metrics
    /// </summary>
    public interface MetricsDataProvider : Utils.IHideObjectMembers
    {
        /// <summary>
        /// Returns the current metrics data for the context for which this provider has been created.
        /// </summary>
        MetricsData CurrentMetricsData { get; }
    }
}
