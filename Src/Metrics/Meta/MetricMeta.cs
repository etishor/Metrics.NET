
namespace Metrics.Meta
{
    public abstract class MetricMeta<T, TMetric, TValue>
        where T : TMetric, MetricValue<TValue>
        where TValue : struct
    {
        private readonly T metric;

        protected MetricMeta(string name, T metric, Unit unit)
        {
            this.metric = metric;
            this.Name = name;
            this.Unit = unit;
        }

        public string Name { get; private set; }
        public TValue Value { get { return this.metric.Value; } }
        public Unit Unit { get; private set; }

        internal TMetric Metric()
        {
            return this.metric;
        }
    }
}
