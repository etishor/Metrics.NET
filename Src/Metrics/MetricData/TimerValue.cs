using Metrics.Utils;

namespace Metrics.MetricData
{
    /// <summary>
    /// The value reported by a Timer Metric
    /// </summary>
    public sealed class TimerValue
    {
        public readonly MeterValue Rate;
        public readonly HistogramValue Histogram;
        public readonly long ActiveSessions;
        private readonly TimeUnit DurationUnit;

        public TimerValue(MeterValue rate, HistogramValue histogram, long activeSessions, TimeUnit durationUnit)
        {
            this.Rate = rate;
            this.Histogram = histogram;
            this.ActiveSessions = activeSessions;
            this.DurationUnit = durationUnit;
        }

        public TimerValue Scale(TimeUnit rate, TimeUnit duration)
        {
            var durationFactor = this.DurationUnit.ScalingFactorFor(duration);
            return new TimerValue(this.Rate.Scale(rate), this.Histogram.Scale(durationFactor), this.ActiveSessions, duration);
        }
    }

    /// <summary>
    /// Combines the value of the timer with the defined unit and the time units for rate and duration.
    /// </summary>
    public class TimerValueSource : MetricValueSource<TimerValue>
    {
        public TimerValueSource(string name, MetricValueProvider<TimerValue> value, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags)
            : base(name, new ScaledValueProvider<TimerValue>(value, v => v.Scale(rateUnit, durationUnit)), unit, tags)
        {
            this.RateUnit = rateUnit;
            this.DurationUnit = durationUnit;
        }

        public TimeUnit RateUnit { get; private set; }
        public TimeUnit DurationUnit { get; private set; }
    }
}
