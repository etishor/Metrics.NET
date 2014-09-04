using System;

namespace Metrics
{
    /// <summary>
    /// A timer is basically a histogram of the duration of a type of event and a meter of the rate of its occurrence.
    /// <seealso cref="Histogram"/> and <seealso cref="Meter"/>
    /// </summary>
    public interface Timer : Utils.IHideObjectMembers
    {
        /// <summary>
        /// Manualy record timer value
        /// </summary>
        /// <param name="time">The value representing the manually measured time.</param>
        /// <param name="unit">Unit for the value.</param>
        void Record(long time, TimeUnit unit);

        /// <summary>
        /// Runs the <paramref name="action"/> and records the time it took.
        /// </summary>
        /// <param name="action">Action to run and record time for.</param>
        void Time(Action action);

        /// <summary>
        /// Runs the <paramref name="action"/> returning the result and records the time it took.
        /// </summary>
        /// <typeparam name="T">Type of the value returned by the action</typeparam>
        /// <param name="action">Action to run and record time for.</param>
        /// <returns>The result of the <paramref name="action"/></returns>
        T Time<T>(Func<T> action);

        /// <summary>
        /// Creates a new disposable instance and records the time it takes until the instance is disposed.
        /// <code>
        /// using(timer.NewContext())
        /// {
        ///     ExecuteMethodThatNeedsMonitoring();
        /// }
        /// </code>
        /// </summary>
        /// <returns>A disposable instance that will record the time passed until disposed.</returns>
        IDisposable NewContext();
    }

    /// <summary>
    /// The value reported by a Timer Metric
    /// </summary>
    public struct TimerValue
    {
        public readonly MeterValue Rate;
        public readonly HistogramValue Histogram;

        public TimerValue(MeterValue rate, HistogramValue histogram)
        {
            this.Rate = rate;
            this.Histogram = histogram;
        }

        public TimerValue Scale(TimeUnit rate, TimeUnit duration)
        {
            return new TimerValue(this.Rate.Scale(rate), this.Histogram.Scale(duration));
        }
    }

    /// <summary>
    /// Combines the value of the timer with the defined unit and the time units for rate and duration.
    /// </summary>
    public class TimerValueSource : MetricValueSource<TimerValue>
    {
        public TimerValueSource(string name, MetricValueProvider<TimerValue> value, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit)
            : base(name, value, unit)
        {
            this.RateUnit = rateUnit;
            this.DurationUnit = durationUnit;
        }

        public TimeUnit RateUnit { get; private set; }
        public TimeUnit DurationUnit { get; private set; }
    }

    /// <summary>
    /// Full data for a Timer metric
    /// </summary>
    public struct TimerData
    {
        public readonly string Name;
        public readonly TimerValue Value;
        public readonly Unit Unit;
        public readonly TimeUnit RateUnit;
        public readonly TimeUnit DurationUnit;

        public TimerData(string name, TimerValue value, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit)
        {
            this.Name = name;
            this.Value = value;
            this.Unit = unit;
            this.RateUnit = rateUnit;
            this.DurationUnit = durationUnit;
        }
    }
}
