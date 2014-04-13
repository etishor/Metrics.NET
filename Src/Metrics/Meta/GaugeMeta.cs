
using Metrics.Core;
namespace Metrics.Meta
{
    public class GaugeMeta : GaugeMeta<GaugeMetric>
    {
        public GaugeMeta(string name, GaugeMetric gauge, Unit unit)
            : base(name, gauge, unit)
        { }
    }

    public class GaugeMeta<T> : MetricMeta<T, Gauge, GaugeValue>
        where T : Gauge, MetricValue<GaugeValue>
    {
        public GaugeMeta(string name, T gauge, Unit unit)
            : base(name, gauge, unit)
        { }
    }
}
