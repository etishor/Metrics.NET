using System;
using System.Linq;

namespace Metrics.StupidBenchmarks
{
    public class BenchmarkResult
    {


        public readonly string Name;
        public readonly string TypeName;

        public readonly int Threads;
        public readonly int TotalSeconds;

        public readonly long RecordedTimeInNano;
        public readonly long[] OperationsPerThread;
        public readonly long[] TimePerThread;

        public BenchmarkResult(string name, string typeName, int threads, int totalSeconds,
            long recordedTimeInNano, long[] operationsperThread, long[] timePerThread)
        {
            this.Name = name;
            this.TypeName = typeName;
            this.Threads = threads;
            this.TotalSeconds = totalSeconds;
            this.RecordedTimeInNano = recordedTimeInNano;
            this.OperationsPerThread = operationsperThread;
            this.TimePerThread = timePerThread;
        }

        public long TotalOperations { get { return this.OperationsPerThread.Sum(); } }
        public long RecordedTime { get { return BenchmarkRunner.NanoToSeconds(this.RecordedTimeInNano); } }

        public long OverallOperationsPerSecond
        {
            get
            {
                if (this.RecordedTime == 0)
                {
                    return -1;
                }
                return this.TotalOperations / RecordedTime;
            }
        }

        public long AverageOperationDuration
        {
            get
            {
                return (long)Math.Round(this.OperationsPerThread.Select((c, i) => this.TimePerThread[i] / (double)c).Average());
            }
        }

        public long AverageOpsPerSecondPerThread
        {
            get
            {
                return (long)Math.Round(this.OperationsPerThread.Select((c, i) =>
                    this.TimePerThread[i] == 0 ? -1 : c / BenchmarkRunner.NanoToSeconds(this.TimePerThread[i])).Average());
            }
        }
    }
}
