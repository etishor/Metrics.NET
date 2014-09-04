
namespace Metrics
{
    /// <summary>
    /// Combines the value of a gauge with the defined unit for the value.
    /// </summary>
    public sealed class GaugeValueSource : MetricValueSource<double>
    {
        public GaugeValueSource(string name, MetricValueProvider<double> value, Unit unit)
            : base(name, value, unit)
        { }
    }

    /// <summary>
    /// Full data for a Gauge metric
    /// </summary>
    public struct GaugeData
    {
        public readonly string Name;
        public readonly double Value;
        public readonly Unit Unit;

        public GaugeData(string name, double value, Unit unit)
        {
            this.Name = name;
            this.Value = value;
            this.Unit = unit;
        }
    }
}
