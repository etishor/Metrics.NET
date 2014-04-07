
namespace Metrics.Meta
{
    public abstract class MetricMeta<TMetric, TMetricValue>
        where TMetric : Metric<TMetricValue>
        where TMetricValue : struct
    {
        private readonly TMetric metric;

        protected MetricMeta(string name, TMetric metric, Unit unit)
        {
            this.metric = metric;
            this.Name = name;
            this.Unit = unit;
        }

        public string Name { get; private set; }
        public TMetricValue Value { get { return this.metric.Value; } }
        public Unit Unit { get; private set; }

        internal TMetric Metric()
        {
            return this.metric;
        }
    }
}
