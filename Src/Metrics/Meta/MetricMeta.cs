
namespace Metrics.Meta
{
    public abstract class MetricMeta<T, V>
        where T : Metric<V>
        where V : struct
    {
        private readonly T metric;

        protected MetricMeta(string name, T metric, Unit unit)
        {
            this.metric = metric;
            this.Name = name;
            this.Unit = unit;
        }

        public string Name { get; private set; }
        public V Value { get { return this.metric.Value; } }
        public Unit Unit { get; private set; }

        internal T Metric()
        {
            return this.metric;
        }
    }
}
