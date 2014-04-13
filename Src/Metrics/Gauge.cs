
namespace Metrics
{
    /// <summary>
    /// A gauge is the simplest metric type. It just returns a value.
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
