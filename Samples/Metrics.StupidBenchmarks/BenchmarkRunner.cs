using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Metrics.StupidBenchmarks
{
    public static class BenchmarkRunner
    {
        public static int DefaultMaxThreads { get; set; }
        public static int DefaultTotalSeconds { get; set; }

        static BenchmarkRunner()
        {
            DefaultMaxThreads = threads.Max();
            DefaultTotalSeconds = 5;
        }

        private static readonly int[] threads = new[] { 1, 2, 4, 6, 8, 16, 32, 48, 64 };

        public static IEnumerable<BenchmarkResult> Run(string name, Action action, int maxThreads = -1, int totalSeconds = -1, int iterationsChunk = 1000)
        {
            var results = new List<BenchmarkResult>(); ;
            foreach (var threadCount in threads.Where(t => t <= (maxThreads == -1 ? DefaultMaxThreads : maxThreads)))
            {
                var runner = new ActionBenchmark(name, threadCount, (totalSeconds == -1 ? DefaultTotalSeconds : totalSeconds), action, iterationsChunk);
                var result = runner.Run();
                results.Add(result);
                Display(result);
            }
            return results;
        }

        private static void Display(BenchmarkResult result)
        {
            Console.WriteLine("{0} {1,2:N0} threads {2,15:N0} ops/sec | avg duration {3,8:N0} ns | {4,10:N0} avg ops/sec per thread", result.Name, result.Threads,
                result.OverallOperationsPerSecond, result.AverageOperationDuration, result.AverageOpsPerSecondPerThread);
        }

        private static readonly long factor = (1000L * 1000L * 1000L) / Stopwatch.Frequency;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long TimeInNanoseconds()
        {
            return Stopwatch.GetTimestamp() * factor;
        }

        public static long SecondsToNano(long seconds)
        {
            return seconds * 1000L * 1000L * 1000L;
        }

        public static long NanoToSeconds(long nano)
        {
            return (long)Math.Round(nano / (double)(1000L * 1000L * 1000L));
        }
    }
}
