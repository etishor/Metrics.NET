using System;

namespace Metrics
{
    /// <summary>
    /// A timer is basically a histogram of the duration of a type of event and a meter of the rate of its occurrence.
    /// <seealso cref="Histogram"/> and <seealso cref="Meter"/>
    /// </summary>
    public interface Timer : Metric<TimerValue>
    {
        void Time(Action action);
        T Time<T>(Func<T> action);
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
}
