using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Metrics.Utils
{
    /// <summary>
    /// Utility class to schedule an Action to be executed repeatedly according to the interval.
    /// </summary>
    /// <remarks>
    /// The scheduling code is inspired form Daniel Crenna's metrics port
    /// https://github.com/danielcrenna/metrics-net/blob/master/src/metrics/Reporting/ReporterBase.cs
    /// </remarks>
    public sealed class ActionScheduler : Scheduler
    {
        private CancellationTokenSource token = null;

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

            if (this.token != null)
            {
                throw new InvalidOperationException("Scheduler is already started.");
            }

            this.token = new CancellationTokenSource();

            RunScheduler(interval, task, this.token);
        }

#if !NET40
        private static void RunScheduler(TimeSpan interval, Func<CancellationToken, Task> action, CancellationTokenSource token)
        {
            Task.Factory.StartNew(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(interval, token.Token);
                    try
                    {
                        await action(token.Token);
                    }
                    catch (Exception x)
                    {
                        HandleException(x);
                        token.Cancel();
                    }
                }
            }, token.Token);
        }
#else // reminds me of the C / C++ days :)
        private static void RunScheduler(TimeSpan interval, Func<CancellationToken, Task> task, CancellationTokenSource token)
        {
            Task.Factory.StartNew(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    TaskUtils.Delay(interval, token.Token).Wait(token.Token);
                    try
                    {
                        task(token.Token).Wait(token.Token);
                    }
                    catch (Exception x)
                    {
                        HandleException(x);
                        token.Cancel();
                    }
                }
            }, token.Token);
        }
#endif

        private static Task Completed()
        {
#if !NET40
            return Task.FromResult(true);
#else
            return TaskUtils.FromResult(true);
#endif
        }

        private static void HandleException(Exception x)
        {
            if (Metric.ErrorHandler != null)
            {
                Metric.ErrorHandler(x);
            }
            else
            {
                Trace.Fail("Got exception while executing scheduler. You can handle this exception by setting a handler on Metric.ErrorHandler", x.ToString());
            }
        }

        public void Stop()
        {
            if (this.token != null)
            {
                token.Cancel();
            }
        }

        private void RunAction(Action<CancellationToken> action)
        {
            try
            {
                action(this.token.Token);
            }
            catch (Exception x)
            {
                if (Metric.ErrorHandler != null)
                {
                    Metric.ErrorHandler(x);
                }
                else
                {
                    throw;
                }
            }
        }

        public void Dispose()
        {
            if (this.token != null)
            {
                this.token.Cancel();
                this.token.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
