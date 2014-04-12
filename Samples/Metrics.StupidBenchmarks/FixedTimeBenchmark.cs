using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Metrics.Utils;

namespace Metrics.StupidBenchmarks
{
    public static class FixedTimeBenchmark
    {
        public static double MeasureCallsPerSecond(Action action, int threadCount, TimeSpan duration)
        {
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
            List<Thread> threads = new List<Thread>();

            long total = 0;

            for (int i = 0; i < threadCount; i++)
            {
                var x = i;
                threads.Add(new Thread(s =>
                {
                    // warm - up
                    for (int j = 0; j < 100; j++) { action(); }

                    tcs.Task.Wait();
                    long count = 0;

#if !NET40
                    var done = new CancellationTokenSource(duration);
#else
                    var done = new CancellationTokenSource();
                    ActionScheduler.Delay(duration.TotalMilliseconds)
                        .ContinueWith(t => done.Cancel());
#endif

                    while (!done.IsCancellationRequested)
                    {
                        action();
                        count++;
                    }
                    Interlocked.Add(ref total, count);
                }));
            }
            // all thread will be waiting on tcs
            threads.ForEach(t => t.Start());
            tcs.SetResult(0);
            threads.ForEach(t => t.Join());
            return total / (double)duration.TotalSeconds;
        }
    }
}
