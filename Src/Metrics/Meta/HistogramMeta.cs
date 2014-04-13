
using Metrics.Core;
namespace Metrics.Meta
{
    public class HistogramMeta : HistogramMeta<HistogramMetric>
    {
        public HistogramMeta(string name, HistogramMetric histogram, Unit unit)
            : base(name, histogram, unit)
        { }
    }

    public class HistogramMeta<T> : MetricMeta<T, Histogram, HistogramValue>
        where T : Histogram, MetricValue<HistogramValue>
    {
        public HistogramMeta(string name, T histogram, Unit unit)
            : base(name, histogram, unit)
        { }
    }
}
