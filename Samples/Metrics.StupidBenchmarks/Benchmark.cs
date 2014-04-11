using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Metrics.StupidBenchmarks
{
    public static class Benchmark
    {
        public struct Result
        {
            public TimeSpan Elapsed;
            public long Total;
            public double PerSecond
            {
                get
                {
                    return Total / Elapsed.TotalSeconds;
                }
            }

            public override string ToString()
            {
                return string.Format("Runs\t{0}\nTotal\t{1}\nRate\t{2}", Total, Elapsed, PerSecond);
            }
        }

        public static Result Run(Action<int, long> action, int threadCount = 8, long iterations = 1 * 1000 * 1000)
        {
#if DEBUG
            Console.WriteLine("DEBUG MODE - results are NOT relevant");
#endif
            // warm - up
            for (int i = 0; i < 100; i++) { action(0, 0); }

            List<Thread> threads = new List<Thread>();
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();

            for (int i = 0; i < threadCount; i++)
            {
                var x = i;
                threads.Add(new Thread(s =>
                    {
                        tcs.Task.Wait();
                        for (long j = 0; j < iterations; j++)
                            action(x, j);
                    }));
            }
            // all thread will be waiting on tcs
            threads.ForEach(t => t.Start());

            var w = Stopwatch.StartNew();
            tcs.SetResult(0);
            threads.ForEach(t => t.Join());
            var elapsed = w.Elapsed;
            return new Result { Elapsed = elapsed, Total = threadCount * iterations };
        }
    }
}
