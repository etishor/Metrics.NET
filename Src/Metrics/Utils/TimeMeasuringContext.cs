using System;

namespace Metrics.Utils
{
    public struct TimeMeasuringContext : TimerContext
    {
        private readonly Clock clock;
        private readonly long start;
        private readonly Action<long> action;
        private bool disposed;

        public TimeMeasuringContext(Clock clock, Action<long> disposeAction)
        {
            this.clock = clock;
            this.start = clock.Nanoseconds;
            this.action = disposeAction;
            this.disposed = false;
        }

        public TimeSpan Elapsed
        {
            get
            {
                var miliseconds = TimeUnit.Nanoseconds.Convert(TimeUnit.Milliseconds, this.clock.Nanoseconds - this.start);
                return TimeSpan.FromMilliseconds(miliseconds);
            }
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }
            this.disposed = true;
            this.action(this.clock.Nanoseconds - this.start);
        }
    }
}
