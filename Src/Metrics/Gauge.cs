
namespace Metrics
{
    /// <summary>
    /// A gauge is the simplest metric type. It just returns a value.
    /// </summary>
    public interface Gauge : Utils.IHideObjectMembers 
    {
    }

    /// <summary>
    /// The value of a gauge, represented as a string.
    /// </summary>
    public struct GaugeValue
    {
        public readonly string Value;

        public GaugeValue(string value)
        {
            this.Value = value;
        }
    }

    /// <summary>
    /// Combines the value of a gauge with the defined unit for the value.
    /// </summary>
    public sealed class GaugeValueSource : MetricValueSource<GaugeValue>
    {
        public GaugeValueSource(string name, MetricValueProvider<GaugeValue> value, Unit unit)
            : base(name, value, unit)
        { }
    }
}
