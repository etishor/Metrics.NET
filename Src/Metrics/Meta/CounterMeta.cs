
using Metrics.Core;
namespace Metrics.Meta
{

    public class CounterMeta : CounterMeta<CounterMetric>
    {
        public CounterMeta(string name, CounterMetric counter, Unit unit)
            : base(name, counter, unit)
        { }
    }

    public class CounterMeta<T> : MetricMeta<T, Counter, long>
        where T : Counter, MetricValue<long>
    {
        public CounterMeta(string name, T counter, Unit unit)
            : base(name, counter, unit)
        { }
    }
}
