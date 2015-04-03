using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Metrics.Core;
using Metrics.MetricData;
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

        public static void Run<T>(Func<T> create, Action<T> action)
        {
            FixedTimeBenchmark.Run(create, action, targetOptions.MaxThreads, targetOptions.Seconds, targetOptions.Decrement);
        }

        static void Main(string[] args)
        {
            var options = new Options();
            if (!Parser.Default.ParseArguments(args, options, (t, o) => { target = t; targetOptions = o as CommonOptions; }))
            {
                Console.WriteLine(new CommonOptions().GetUsage());
                Environment.Exit(CommandLine.Parser.DefaultExitCodeFail);
            }

            Console.WriteLine("{0} | Duration {1} seconds  | Start Threads {2} | Step {3}", target, targetOptions.Seconds, targetOptions.MaxThreads, targetOptions.Decrement);

            CounterMetric counter = new CounterMetric();
            MeterMetric meter = new MeterMetric();

            CancellationTokenSource tcs = new CancellationTokenSource();

            List<MeterValue> values = new List<MeterValue>();

            var reader = Task.Factory.StartNew(async () =>
            {
                while (!tcs.IsCancellationRequested)
                {
                    values.Add(meter.Value);
                    await Task.Delay(200);
                    if (values.Count > 100)
                    {
                        values.Clear();
                    }
                }
            });

            switch (target)
            {
                case "counter":
                    Run(() => counter, c => c.Increment());
                    break;
                case "meter":
                    Run(() => meter, m => m.Mark());
                    break;
                case "histogram":
                    Run(() => new HistogramMetric(), h => h.Update(37));
                    break;
                case "timer":
                    Run(() => new TimerMetric(SamplingType.FavourRecent), t => t.Record(10, TimeUnit.Milliseconds));
                    break;
                case "ewma":
                    Run(() => EWMA.OneMinuteEWMA(), m => m.Update(1));
                    break;
                case "edr":
                    Run(() => new ExponentiallyDecayingReservoir(), r => r.Update(100));
                    break;
                case "uniform":
                    Run(() => new UniformReservoir(), r => r.Update(100));
                    break;
                case "sliding":
                    Run(() => new SlidingWindowReservoir(), r => r.Update(100));
                    break;
            }

            Console.WriteLine(values.Count);
            tcs.Cancel();
        }
    }
}
