
namespace Metrics
{
    /// <summary>
    /// A gauge is the simplest metric type. It just returns a value.
    /// </summary>
    public interface Gauge : Metric<GaugeValue>
    {
    }

    public struct GaugeValue
    {
        public readonly string Value;

        public GaugeValue(string value)
        {
            this.Value = value;
        }
    }
}
