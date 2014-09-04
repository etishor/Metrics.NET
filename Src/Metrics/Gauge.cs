
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
}
