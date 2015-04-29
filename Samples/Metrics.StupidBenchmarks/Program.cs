using System;
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

        [VerbOption("TimerImpact")]
        public CommonOptions TimerImpact { get; set; }

        [VerbOption("NoOp")]
        public CommonOptions NoOp { get; set; }

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

            BenchmarkRunner.DefaultTotalSeconds = targetOptions.Seconds;
            BenchmarkRunner.DefaultMaxThreads = targetOptions.MaxThreads;

            //Metric.Config.WithHttpEndpoint("http://localhost:1234/");

            switch (target)
            {
                case "noop":
                    BenchmarkRunner.Run("Noop", () => { });
                    break;
                case "counter":
                    var counter = new CounterMetric();
                    BenchmarkRunner.Run("Counter", () => counter.Increment());
                    break;
                case "meter":
                    var meter = new MeterMetric();
                    BenchmarkRunner.Run("Meter", () => meter.Mark());
                    break;
                case "histogram":
                    var histogram = new HistogramMetric();
                    BenchmarkRunner.Run("Histogram", () => histogram.Update(137));
                    break;
                case "timer":
                    var timer = new TimerMetric();
                    BenchmarkRunner.Run("Timer", () => timer.Record(1, TimeUnit.Milliseconds));
                    break;
                case "hdrtimer":
                    var hdrTimer = new TimerMetric(new HdrHistogramReservoir());
                    BenchmarkRunner.Run("HDR Timer", () => hdrTimer.Record(1, TimeUnit.Milliseconds));
                    break;
                case "ewma":
                    var ewma = EWMA.OneMinuteEWMA();
                    BenchmarkRunner.Run("EWMA", () => ewma.Update(1));
                    break;
                case "edr":
                    var edr = new ExponentiallyDecayingReservoir();
                    BenchmarkRunner.Run("EDR", () => edr.Update(1));
                    break;
                case "hdr":
                    var hdrReservoir = new HdrHistogramReservoir();
                    BenchmarkRunner.Run("HDR Recorder", () => hdrReservoir.Update(1));
                    break;
                case "hdrsync":
                    var hdrSyncReservoir = new SyncronizedHdrReservoir();
                    BenchmarkRunner.Run("HDR Sync", () => hdrSyncReservoir.Update(1));
                    break;
                case "hdrsynctimer":
                    var hdrSyncTimer = new TimerMetric(new SyncronizedHdrReservoir());
                    BenchmarkRunner.Run("HDR Sync Timer", () => hdrSyncTimer.Record(1, TimeUnit.Milliseconds));
                    break;
                case "uniform":
                    var uniform = new UniformReservoir();
                    BenchmarkRunner.Run("Uniform", () => uniform.Update(1));
                    break;
                case "sliding":
                    var sliding = new SlidingWindowReservoir();
                    BenchmarkRunner.Run("Sliding", () => sliding.Update(1));
                    break;
                case "timerimpact":
                    WorkLoad load = new WorkLoad();
                    BenchmarkRunner.Run("WorkWithoutTimer", () => load.DoSomeWork(), iterationsChunk: 10);
                    BenchmarkRunner.Run("WorkWithTimer", () => load.DoSomeWorkWithATimer(), iterationsChunk: 10);
                    break;
            }
        }
    }
}
