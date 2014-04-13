
using Metrics.Core;
namespace Metrics.Meta
{
    public class MeterMeta : MeterMeta<MeterMetric>
    {
        public MeterMeta(string name, MeterMetric meter, Unit unit, TimeUnit rateUnit)
            : base(name, meter, unit, rateUnit)
        { }
    }

    public class MeterMeta<T> : MetricMeta<T, Meter, MeterValue>
        where T : Meter, MetricValue<MeterValue>
    {
        public MeterMeta(string name, T meter, Unit unit, TimeUnit rateUnit)
            : base(name, meter, unit)
        {
            this.RateUnit = rateUnit;
        }

        public TimeUnit RateUnit { get; private set; }
    }
}
