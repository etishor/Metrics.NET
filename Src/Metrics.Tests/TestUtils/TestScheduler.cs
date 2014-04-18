using System;
using System.Threading;
using System.Threading.Tasks;
using Metrics.Utils;

namespace Metrics.Tests.TestUtils
{
    /// <summary>
    /// Utility class for manually executing the scheduled task.
    /// </summary>
    /// <remarks>
    /// This class is useful for testing.
    /// </remarks>
    public sealed class TestScheduler : Scheduler
    {
        private readonly TestClock clock;
        private TimeSpan interval;
        private Action<CancellationToken> action;
        private long lastRun = 0;

        public TestScheduler(TestClock clock)
        {
            this.clock = clock;
            this.clock.Advanced += (s, l) => this.RunIfNeeded();
        }

        public void Start(TimeSpan interval, Func<CancellationToken, Task> task)
        {
            Start(interval, (t) => task(t).Wait());
        }

        public void Start(TimeSpan interval, Func<Task> task)
        {
            Start(interval, () => task().Wait());
        }

        public void Start(TimeSpan interval, Action action)
        {
            Start(interval, t => action());
        }

        public void Start(TimeSpan interval, Action<CancellationToken> action)
        {
            if (interval.TotalSeconds == 0)
            {
                throw new ArgumentException("interval must be > 0 seconds", "interval");
            }

            this.interval = interval;
            this.lastRun = this.clock.Seconds;
            this.action = action;
        }

        private void RunIfNeeded()
        {
            long elapsed = clock.Seconds - lastRun;
            var times = elapsed / interval.TotalSeconds;
            using (CancellationTokenSource ts = new CancellationTokenSource())
                while (times-- > 0)
                    action(ts.Token);
            lastRun = clock.Seconds;
        }

        public void Stop() { }
        public void Dispose() { }
    }

}
