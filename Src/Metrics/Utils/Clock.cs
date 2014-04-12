
using System;

namespace Metrics.Utils
{
    public abstract class Clock
    {
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
            }
        }

        public static readonly Clock System = new SystemClock();

        public abstract long Nanoseconds { get; }

        public long Seconds { get { return TimeUnit.Nanoseconds.ToSeconds(Nanoseconds); } }
    }
}
