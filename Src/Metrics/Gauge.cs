
namespace Metrics
{
    /// <summary>
    /// A gauge is the simplest metric type. It just represents a value.
    /// No operation can be triggered on the metric directly. 
    /// Custom implementations can hook into any value provider.
    /// <see cref="Core.FunctionGauge"/> and <see cref="Core.DerivedGauge"/>
    /// </summary>
    public interface Gauge : Utils.IHideObjectMembers
    {
    }

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
