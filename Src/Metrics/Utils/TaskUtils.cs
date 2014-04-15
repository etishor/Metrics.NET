#if NET40
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Metrics.Utils
{
    public static class TaskUtils
    {
        public static Task FromResult<T>(T result)
        {
            var tcs = new TaskCompletionSource<bool>();
            tcs.SetResult(true);
            return tcs.Task;
        }

        public static Task Delay(TimeSpan duration)
        {
            return Delay(duration, CancellationToken.None);
        }

        public static Task Delay(TimeSpan duration, CancellationToken token)
        {
            var tcs = new TaskCompletionSource<bool>();
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += (obj, args) =>
            {
                tcs.SetResult(true);
                timer.Dispose();
            };
            timer.Interval = duration.TotalMilliseconds;
            timer.AutoReset = false;
            timer.Start();
            return tcs.Task;
        }
    }
}
#endif