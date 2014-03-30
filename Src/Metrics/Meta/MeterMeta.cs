
namespace Metrics.Meta
{
    public class MeterMeta : MetricMeta<Meter, MeterValue>
    {
        public MeterMeta(string name, Meter meter, Unit unit, TimeUnit rateUnit)
            : base(name, meter, unit)
        {
            this.RateUnit = rateUnit;
        }

        public TimeUnit RateUnit { get; private set; }
    }
}
