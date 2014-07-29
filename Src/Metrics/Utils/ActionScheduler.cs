using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Metrics.Utils
{
    using Timer = System.Timers.Timer;

    /// <summary>
    /// Utility class to schedule an Action to be executed repeatedly according to the interval.
    /// </summary>
    /// <remarks>
    /// The scheduling code is inspired form Daniel Crenna's metrics port
    /// https://github.com/danielcrenna/metrics-net/blob/master/src/metrics/Reporting/ReporterBase.cs
    /// </remarks>
    public sealed class ActionScheduler : Scheduler
    {
        private readonly Timer timer = new Timer();

        public void Start(TimeSpan interval, Action action)
        {
            Start(interval, t =>
            {
                if (!t.IsCancellationRequested)
                {
                    action();
                }
            });
        }

        public void Start(TimeSpan interval, Action<CancellationToken> action)
        {
            Start(interval, t =>
            {
                action(t);
                return Completed();
            });
        }

        public void Start(TimeSpan interval, Func<Task> task)
        {
            Start(interval, t => t.IsCancellationRequested ? task() : Completed());
        }

        public void Start(TimeSpan interval, Func<CancellationToken, Task> task)
        {
            if (interval.TotalSeconds == 0)
            {
                throw new ArgumentException("interval must be > 0 seconds", "interval");
            }

            this.timer.Elapsed += (obj, args) =>
            {
                try
                {
                    task(CancellationToken.None).Wait();
                }
                catch (Exception x)
                {
                    HandleException(x);
                    timer.Stop();
                }
            };

            timer.Interval = interval.TotalMilliseconds;
            timer.Start();
        }

        private static Task Completed()
        {
            var taskSource = new TaskCompletionSource<bool>();
            taskSource.SetResult(true);
            return taskSource.Task;
        }

        private static void HandleException(Exception x)
        {
            if (Metric.Config.ErrorHandler != null)
            {
                Metric.Config.ErrorHandler(x);
            }
            else
            {
                Trace.Fail("Got exception while executing scheduler. You can handle this exception by setting a handler on Metric.ErrorHandler", x.ToString());
            }
        }

        public void Stop()
        {
            this.timer.Stop();
        }

        public void Dispose()
        {
            this.timer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
