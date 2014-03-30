
namespace Metrics.Meta
{
    public class CounterMeta : MetricMeta<Counter, long>
    {
        public CounterMeta(string name, Counter counter, Unit unit)
            : base(name, counter, unit)
        { }
    }
}
