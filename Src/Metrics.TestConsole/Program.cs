
using System;
using Metrics.Sampling;
using Metrics.Utils;

namespace Metrics.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Metric.Config.WithHttpEndpoint(@"http://localhost:1234/");

            var maxValue = TimeUnit.Hours.Convert(TimeUnit.Nanoseconds, 1);
            var histogram = new HdrHistogram.NET.Histogram(maxValue, 3);

            var p5 = histogram.getValueAtPercentile(50);

            Reservoir reservoir = new SyncronizedHdrReservoir(histogram);

            var hdr = Metric.Advanced.Histogram("hdr", Unit.Calls, () => reservoir);



            Console.WriteLine("press any key to exit");
            Console.ReadKey();
        }
    }
}
