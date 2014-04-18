using System;
using Metrics.Utils;

namespace Metrics.Tests.TestUtils
{
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
}
