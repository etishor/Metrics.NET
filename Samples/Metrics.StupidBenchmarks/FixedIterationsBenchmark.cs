using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Metrics.StupidBenchmarks
{
    public static class FixedIterationsBenchmark
    {
        public struct Result
        {
            public TimeSpan Elapsed;
            public long Total;
            public long PerSecond
            {
                get
                {
                    return Convert.ToInt64(Math.Round(Total / Elapsed.TotalSeconds));
                }
            }

            public override string ToString()
            {
                return string.Format("Runs\t{0}\nTotal\t{1}\nRate\t{2}", Total, Elapsed, PerSecond);
            }
        }

        public static void Run<T>(Action<T> action, long iterations, int maxThreads = 16)
           where T : new()
        {
            T instance = new T();
            for (int i = 1; i < maxThreads; i++)
            {
                var result = FixedIterationsBenchmark.MeasureDuration(() => action(instance), i, iterations);
                Console.WriteLine("{0}\t{1}\t{2}\t{3}", typeof(T).Name, i, result.PerSecond, result.Elapsed);
            }
        }


        public static Result MeasureDuration(Action action, int threadCount, long iterations)
        {
#if DEBUG
            Console.WriteLine("DEBUG MODE - results are NOT relevant");
#endif
            List<Thread> threads = new List<Thread>();
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();

            for (int i = 0; i < threadCount; i++)
            {
                threads.Add(new Thread(s =>
                    {
                        // warm - up
                        for (int x = 0; x < 100; x++) { action(); }

                        tcs.Task.Wait();
                        for (long j = 0; j < iterations; j++)
                        {
                            action();
                        }
                    }));
            }
            // all thread will be waiting on tcs
            threads.ForEach(t => t.Start());

            var w = Stopwatch.StartNew();
            tcs.SetResult(0);
            threads.ForEach(t => t.Join());
            var elapsed = w.Elapsed;
            return new Result { Elapsed = elapsed, Total = iterations };
        }
    }
}
