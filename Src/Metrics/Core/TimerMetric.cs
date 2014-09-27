using System;
using Metrics.Sampling;
using Metrics.Utils;

namespace Metrics.Core
{
    public interface TimerImplementation : Timer, MetricValueProvider<TimerValue> { }

    public sealed class TimerMetric : TimerImplementation, IDisposable
    {
        private readonly Clock clock;
        private readonly Meter meter;
        private readonly Histogram histogram;
        private readonly MetricValueProvider<MeterValue> meterValue;
        private readonly MetricValueProvider<HistogramValue> histogramValue;

        public TimerMetric()
            : this(new HistogramMetric(), new MeterMetric(), Clock.Default) { }

        public TimerMetric(SamplingType samplingType)
            : this(new HistogramMetric(samplingType), new MeterMetric(), Clock.Default) { }

        public TimerMetric(Histogram histogram)
            : this(histogram, new MeterMetric(), Clock.Default) { }

        public TimerMetric(Reservoir reservoir)
            : this(new HistogramMetric(reservoir), new MeterMetric(), Clock.Default) { }

        public TimerMetric(SamplingType samplingType, Meter meter, Clock clock)
            : this(new HistogramMetric(samplingType), meter, clock) { }

        public TimerMetric(Histogram histogram, Meter meter, Clock clock)
        {
            this.clock = clock;
            this.meter = meter;
            this.histogram = histogram;
            this.meterValue = meter as MetricValueProvider<MeterValue>;

            if (meterValue == null)
            {
                throw new InvalidOperationException("Meter type must also implement MetricValue<MeterValue>");
            }

            this.histogramValue = histogram as MetricValueProvider<HistogramValue>;

            if (histogramValue == null)
            {
                throw new InvalidOperationException("Histogram type must also implement MetricValue<HistogramValue>");
            }
        }

        public void Record(long duration, TimeUnit unit, string userValue = null)
        {
            var nanos = unit.ToNanoseconds(duration);
            if (nanos >= 0)
            {
                this.histogram.Update(nanos, userValue);
                this.meter.Mark();
            }
        }

        public void Time(Action action, string userValue = null)
        {
            var start = this.clock.Nanoseconds;
            try
            {
                action();
            }
            finally
            {
                Record(this.clock.Nanoseconds - start, TimeUnit.Nanoseconds, userValue);
            }
        }

        public T Time<T>(Func<T> action, string userValue = null)
        {
            var start = this.clock.Nanoseconds;
            try
            {
                return action();
            }
            finally
            {
                Record(this.clock.Nanoseconds - start, TimeUnit.Nanoseconds, userValue);
            }
        }

        public TimerContext NewContext(string userValue = null)
        {
            return new TimeMeasuringContext(this.clock, (t) => Record(t, TimeUnit.Nanoseconds, userValue));
        }

        public TimerContext NewContext(Action<TimeSpan> finalAction, string userValue = null)
        {
            return new TimeMeasuringContext(this.clock, (t) =>
            {
                Record(t, TimeUnit.Nanoseconds, userValue);
                finalAction(TimeSpan.FromMilliseconds(TimeUnit.Nanoseconds.ToMilliseconds(t)));
            });
        }

        public TimerValue Value
        {
            get
            {
                return new TimerValue(this.meterValue.Value, this.histogramValue.Value);
            }
        }

        public void Reset()
        {
            this.meter.Reset();
            this.histogram.Reset();
        }

        public void Dispose()
        {
            using (this.histogram as IDisposable) { }
            using (this.meter as IDisposable) { }
        }
    }
}
