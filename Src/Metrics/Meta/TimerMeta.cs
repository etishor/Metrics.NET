
namespace Metrics.Meta
{
    public class TimerMeta : MetricMeta<Timer, TimerValue>
    {
        public TimerMeta(string name, Timer timer, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit)
            : base(name, timer, unit)
        {
            this.RateUnit = rateUnit;
            this.DurationUnit = durationUnit;
        }

        public TimeUnit RateUnit { get; private set; }
        public TimeUnit DurationUnit { get; private set; }
    }
}
