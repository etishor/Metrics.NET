
using Metrics.Core;
namespace Metrics.Meta
{
    public class TimerMeta : TimerMeta<TimerMetric>
    {
        public TimerMeta(string name, TimerMetric timer, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit)
            : base(name, timer, unit, rateUnit, durationUnit)
        { }
    }

    public class TimerMeta<T> : MetricMeta<T, Timer, TimerValue>
        where T : Timer, MetricValue<TimerValue>
    {
        public TimerMeta(string name, T timer, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit)
            : base(name, timer, unit)
        {
            this.RateUnit = rateUnit;
            this.DurationUnit = durationUnit;
        }

        public TimeUnit RateUnit { get; private set; }
        public TimeUnit DurationUnit { get; private set; }
    }
}
