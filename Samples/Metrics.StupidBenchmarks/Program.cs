using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Metrics.Core;
using Metrics.Sampling;
using Metrics.Utils;
namespace Metrics.StupidBenchmarks
{
    class CommonOptions
    {
        [Option('c', HelpText = "Max Threads", DefaultValue = 32)]
        public int MaxThreads { get; set; }

        [Option('s', HelpText = "Seconds", DefaultValue = 5)]
        public int Seconds { get; set; }

        [Option('d', HelpText = "Number of threads to decrement each step", DefaultValue = 4)]
        public int Decrement { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this);
        }
    }

    class Options
    {
        [VerbOption("Counter")]
        public CommonOptions Counter { get; set; }

        [VerbOption("Meter")]
        public CommonOptions Meter { get; set; }

        [VerbOption("Histogram")]
        public CommonOptions Histogram { get; set; }

        [VerbOption("Timer")]
        public CommonOptions Timer { get; set; }

        [VerbOption("EWMA")]
        public CommonOptions Ewma { get; set; }

        [VerbOption("EDR")]
        public CommonOptions Edr { get; set; }

        [VerbOption("hdr")]
        public CommonOptions Hdr { get; set; }

        [VerbOption("hdrtimer")]
        public CommonOptions HdrTimer { get; set; }

        [VerbOption("hdrsync")]
        public CommonOptions HdrSync { get; set; }

        [VerbOption("hdrsynctimer")]
        public CommonOptions HdrSyncTimer { get; set; }

        [VerbOption("Uniform")]
        public CommonOptions Uniform { get; set; }

        [VerbOption("Sliding")]
        public CommonOptions Sliding { get; set; }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            return HelpText.AutoBuild(this);
        }
    }

    class Program
    {
        private static string target;
        private static CommonOptions targetOptions;

        static void Main(string[] args)
        {
            var options = new Options();
            if (!Parser.Default.ParseArguments(args, options, (t, o) => { target = t; targetOptions = o as CommonOptions; }))
            {
                Console.WriteLine(new CommonOptions().GetUsage());
                Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
            }

            //Console.WriteLine("{0} | Duration {1} seconds  | Start Threads {2} | Step {3}", target, targetOptions.Seconds, targetOptions.MaxThreads, targetOptions.Decrement);

            BenchmarkRunner.DefaultTotalSeconds = targetOptions.Seconds;
            BenchmarkRunner.DefaultMaxThreads = targetOptions.MaxThreads;

            //BenchmarkRunner.Run("HDR", c => c.recordValue(137), () => new HdrHistogram.Histogram(1, BenchmarkRunner.SecondsToNano(200), 2));

            switch (target)
            {
                case "counter":
                    BenchmarkRunner.Run("Counter", c => c.Increment(), () => new CounterMetric());
                    break;
                case "meter":
                    BenchmarkRunner.Run("Meter", c => c.Mark(), () => new MeterMetric());
                    break;
                case "histogram":
                    BenchmarkRunner.Run("Histogram", c => c.Update(137), () => new HistogramMetric());
                    break;
                case "timer":
                    BenchmarkRunner.Run("Timer", c => c.Record(137, TimeUnit.Milliseconds), () => new TimerMetric());
                    break;
                case "ewma":
                    BenchmarkRunner.Run("EWMA", c => c.Update(1), () => EWMA.OneMinuteEWMA());
                    break;
                case "edr":
                    BenchmarkRunner.Run("EDR", c => c.Update(137), () => new ExponentiallyDecayingReservoir());
                    break;
                case "hdr":
                    BenchmarkRunner.Run("HDR Recorder", c => c.Update(137), () => new HdrHistogramReservoir());
                    break;
                case "hdrsync":
                    BenchmarkRunner.Run("HDR Sync", c => c.Update(137), () => new SyncronizedHdrReservoir());
                    break;
                case "hdrtimer":
                    BenchmarkRunner.Run("HDR Timer", c => c.Record(137, TimeUnit.Milliseconds), () => new TimerMetric(new HdrHistogramReservoir()));
                    break;
                case "hdrsynctimer":
                    BenchmarkRunner.Run("HDR Sync Timer", c => c.Record(137, TimeUnit.Milliseconds), () => new TimerMetric(new SyncronizedHdrReservoir()));
                    break;
                case "uniform":
                    BenchmarkRunner.Run("Uniform", c => c.Update(137), () => new UniformReservoir());
                    break;
                case "sliding":
                    BenchmarkRunner.Run("Sliding", c => c.Update(137), () => new SlidingWindowReservoir());
                    break;
            }
        }

        private static Task ReaderTask<T>(Func<T> reader, CancellationToken token)
        {
            List<T> values = new List<T>();

            return Task.Factory.StartNew(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    values.Add(reader());
                    await Task.Delay(200);
                    if (values.Count > 100)
                    {
                        values.Clear();
                    }
                }
            });
        }
    }
}
