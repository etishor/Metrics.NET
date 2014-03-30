
namespace Metrics.Meta
{
    public class GaugeMeta : MetricMeta<Gauge, GaugeValue>
    {
        public GaugeMeta(string name, Gauge gauge, Unit unit)
            : base(name, gauge, unit)
        { }
    }
}
