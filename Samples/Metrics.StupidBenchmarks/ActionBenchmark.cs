

using System;
using System.Threading;

namespace Metrics.StupidBenchmarks
{
    public class ActionBenchmark<T>
    {
        private readonly string name;
        private readonly int threadCount;
        private readonly int totalSeconds;
        private readonly Action<T> action;
        private readonly Func<T> builder;

        private readonly EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private readonly long totalTimeNanoseconds;
        private readonly EventWaitHandle[] threadHandles;
        private readonly long[] threadTimes;
        private readonly long[] threadCounts;

        private readonly int iterationsChunk;

        private readonly T testInstance;
        private readonly Thread[] threads;

        public ActionBenchmark(string name, int threadCount, int totalSeconds, Action<T> action, Func<T> builder, int iterationsChunk = 100000)
        {
            this.name = name;
            this.threadCount = threadCount;
            this.totalSeconds = totalSeconds;
            this.action = action;
            this.builder = builder;
            this.iterationsChunk = iterationsChunk;

            this.totalTimeNanoseconds = BenchmarkRunner.SecondsToNano(totalSeconds);

            this.testInstance = this.builder();
            this.threads = new Thread[threadCount];
            this.threadHandles = new EventWaitHandle[threadCount];
            this.threadTimes = new long[threadCount];
            this.threadCounts = new long[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                var threadNumber = i;
                this.threads[i] = new Thread(() => RunAction(threadNumber));
                this.threads[i].Start();
                this.threadHandles[i] = new EventWaitHandle(false, EventResetMode.ManualReset);
            }
        }

        public BenchmarkResult Run()
        {
            WarmUp();

            this.waitHandle.Set();
            var start = BenchmarkRunner.TimeInNanoseconds();
            WaitHandle.WaitAll(this.threadHandles);
            var end = BenchmarkRunner.TimeInNanoseconds();

            var recordedTime = end - start;
            foreach (var thread in threads)
            {
                thread.Join();
            }

            var result = new BenchmarkResult(this.name, typeof(T).Name, this.threadCount, this.totalSeconds, recordedTime, this.threadCounts, this.threadTimes);
            PerformCollection();
            return result;
        }

        private void RunAction(int threadNumber)
        {
            this.action(this.testInstance);
            this.waitHandle.WaitOne();
            var start = BenchmarkRunner.TimeInNanoseconds();
            var end = start;
            long count = 0;

            while (end - start < this.totalTimeNanoseconds)
            {
                for (int i = 0; i < this.iterationsChunk; i++)
                {
                    this.action(this.testInstance);
                }
                count += this.iterationsChunk;
                end = BenchmarkRunner.TimeInNanoseconds();
            }
            this.threadHandles[threadNumber].Set();
            this.threadTimes[threadNumber] = end - start;
            this.threadCounts[threadNumber] = count;
        }

        private void WarmUp()
        {
            var instance = this.builder();
            action(instance);
            action(instance);
            action(instance);
            action(instance);
            action(instance);
            PerformCollection();
        }

        private static void PerformCollection()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Thread.Sleep(200);
        }


    }
}
