using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Metrics.StupidBenchmarks
{
    public static class FixedTimeBenchmark
    {
        public static void Run<T>(Func<T> create, Action<T> action, int maxThreads = 16, int seconds = 5, int decrement = 1)
        {
            T instance = create();
            for (int i = maxThreads; i > 0; i -= decrement)
            {
                var result = FixedTimeBenchmark.MeasureCallsPerSecond(() => action(instance), i, TimeSpan.FromSeconds(seconds));
                Console.WriteLine("{0}\t{1}\t{2,10:N0}", typeof(T).Name, i, result);
            }
        }

        public static void Run<T>(Action<T> action, int maxThreads = 16, int seconds = 5, int decrement = 1)
           where T : new()
        {
            Run<T>(() => new T(), action, maxThreads, seconds, decrement);
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

                    var done = new CancellationTokenSource(duration);

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
