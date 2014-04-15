
using System;

namespace Metrics.Utils
{
    public abstract class Clock
    {
        private sealed class StopwatchClock : Clock
        {
            private static readonly long factor = (1000L * 1000L * 1000L) / System.Diagnostics.Stopwatch.Frequency;
            public override long Nanoseconds { get { return System.Diagnostics.Stopwatch.GetTimestamp() * factor; } }
        }

        private sealed class SystemClock : Clock
        {
            public override long Nanoseconds { get { return DateTime.UtcNow.Ticks * 100L; } }
        }

        public sealed class TestClock : Clock
        {
            private long nanoseconds = 0;
            public override long Nanoseconds { get { return this.nanoseconds; } }
            public void Advance(TimeUnit unit, long value)
            {
                this.nanoseconds += unit.ToNanoseconds(value);
                if (Advanced != null)
                {
                    Advanced(this, EventArgs.Empty);
                }
            }

            public event EventHandler Advanced;
        }

        public static readonly Clock SystemDateTime = new SystemClock();
        public static readonly Clock Default = new StopwatchClock();

        public abstract long Nanoseconds { get; }

        public long Seconds { get { return TimeUnit.Nanoseconds.ToSeconds(Nanoseconds); } }
    }
}
