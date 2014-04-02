using System;
using System.Diagnostics;
using Metrics.Utils;

namespace Metrics.Core
{
    public class TimerMetric : Timer
    {
        private sealed class Context : IDisposable
        {
            private readonly TimerMetric timer;
            private readonly long startTime;
            private bool disposed = false;

            public Context(TimerMetric timer)
            {
                this.timer = timer;
                this.startTime = timer.clock.Nanoseconds;
            }

            public void Dispose()
            {
                if (!disposed)
                {
                    this.timer.Update(this.timer.clock.Nanoseconds - this.startTime, TimeUnit.Nanoseconds);
                }
                disposed = true;
            }
        }

        private readonly Clock clock;
        private readonly Meter meter;
        private readonly Histogram histogram;

        public TimerMetric()
            : this(new HistogramMetric(), Clock.System) { }

        public TimerMetric(SamplingType samplingType)
            : this(new HistogramMetric(samplingType), Clock.System) { }

        public TimerMetric(SamplingType samplingType, Clock clock)
            : this(new HistogramMetric(samplingType), clock) { }

        public TimerMetric(Histogram histogram, Clock clock)
        {
            this.clock = clock;
            this.meter = new MeterMetric(clock);
            this.histogram = histogram;
        }

        public void Time(Action action)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                action();
            }
            finally
            {
                Update(watch.ElapsedTicks * (1000L * 1000L * 1000L) / Stopwatch.Frequency, TimeUnit.Nanoseconds);
            }
        }

        public T Time<T>(Func<T> action)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                return action();
            }
            finally
            {
                Update(watch.ElapsedTicks * (1000L * 1000L * 1000L) / Stopwatch.Frequency, TimeUnit.Nanoseconds);
            }
        }

        public IDisposable NewContext()
        {
            return new Context(this);
        }

        public TimerValue Value
        {
            get
            {
                return new TimerValue(this.meter.Value, this.histogram.Value);
            }
        }

        private void Update(long duration, TimeUnit unit)
        {
            var nanos = unit.ToNanoseconds(duration);
            if (nanos >= 0)
            {
                this.histogram.Update(nanos);
                this.meter.Mark();
            }
        }
    }
}
