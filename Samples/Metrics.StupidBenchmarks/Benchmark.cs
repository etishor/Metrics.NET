using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Metrics.Core;

namespace Metrics.StupidBenchmarks
{
    public static class Benchmark
    {
        public static void Run()
        {
            const int threadCount = 32;
            const long iterations = 1000000;// *1000;


            Timer timer = new TimerMetric();

            for (int i = 0; i < iterations; i++ )
            {
                timer.Time(() => { Math.Sin(i * i); });
            }

            List<Thread> threads = new List<Thread>();

            for (int i = 0; i < threadCount; i++)
            {
                threads.Add(new Thread(s =>
                {
                    Thread.Sleep(100);
                    for (long j = 0; j < iterations; j++)
                    {
                        timer.Time(() => { Math.Sin(i * j); });
                    }
                }));
            }

            var w = Stopwatch.StartNew();
            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());
            var elapsed = w.Elapsed;
            Console.WriteLine(elapsed);
            Console.WriteLine(threadCount * iterations / elapsed.TotalSeconds);

        }
    }
}
