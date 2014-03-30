
namespace Metrics.Meta
{
    public class HistogramMeta : MetricMeta<Histogram, HistogramValue>
    {
        public HistogramMeta(string name, Histogram histogram, Unit unit)
            : base(name, histogram, unit)
        { }
    }
}
