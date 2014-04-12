using System;
using System.Threading;

namespace Metrics.Utils
{
    /// <summary>
    /// Utility class for manualy executing the scheuled task.
    /// </summary>
    /// <remarks>
    /// This class is usefull for testing.
    /// </remarks>
    public sealed class ManualScheduler : Scheduler
    {
        private readonly Clock clock;
        private TimeSpan interval;
        private Action<CancellationToken> action;
        private long lastRun = 0;

        public ManualScheduler(Clock clock)
        {
            this.clock = clock;
        }

        public void Start(TimeSpan interval, Action action)
        {
            Start(interval, t => action());
        }

        public void Start(TimeSpan interval, Action<CancellationToken> action)
        {
            this.interval = interval;
            this.lastRun = this.clock.Seconds;
            this.action = action;
        }

        public void RunIfNeeded()
        {
            long elapsed = clock.Seconds - lastRun;
            var times = elapsed / interval.Seconds;
            using (CancellationTokenSource ts = new CancellationTokenSource())
                while (times-- > 0)
                    action(ts.Token);
            lastRun = clock.Seconds;
        }

        public void Stop() { }
        public void Dispose() { }
    }

}
