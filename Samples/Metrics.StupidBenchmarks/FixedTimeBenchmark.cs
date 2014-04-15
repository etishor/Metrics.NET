using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#if NET40
using Metrics.Utils;
#endif

namespace Metrics.StupidBenchmarks
{
    public static class FixedTimeBenchmark
    {
        public static void Run<T>(Action<T> action, int maxThreads = 16, int seconds = 2)
           where T : new()
        {
            T instance = new T();
            for (int i = maxThreads; i > 0; i--)
            {
                var result = FixedTimeBenchmark.MeasureCallsPerSecond(() => action(instance), i, TimeSpan.FromSeconds(seconds));
                Console.WriteLine("{0}\t{1}\t{2,10:N0}", typeof(T).Name, i, result);
            }
        }

        public static long MeasureCallsPerSecond(Action action, int threadCount, TimeSpan duration)
        {
#if DEBUG
            Console.WriteLine("DEBUG MODE - results are NOT relevant");
#endif
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
            List<Thread> threads = new List<Thread>();

            long total = 0;

            for (int i = 0; i < threadCount; i++)
            {
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
                    TaskUtils.Delay(duration)
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
            return Convert.ToInt64(Math.Round(total / duration.TotalSeconds));
        }
    }
}
