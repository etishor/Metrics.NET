
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Metrics.Sampling;
using Metrics.Utils;

namespace Metrics.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Metric.Config.WithHttpEndpoint(@"http://localhost:1234/");

            //var maxValue = TimeUnit.Hours.Convert(TimeUnit.Nanoseconds, 1);
            //var histogram = new HdrHistogram.Histogram(maxValue, 3);

            //Reservoir reservoir = new SyncronizedHdrReservoir(histogram);

            //var hdr = Metric.Advanced.Histogram("hdr", Unit.Calls, () => reservoir);

            //hdr.Update(1);

            ////var histogram = new HdrHistogram.Histogram(2);
            //var timer = Metric.Advanced.Timer("hdr", Unit.Calls, () => (Reservoir)new HdrHistogramReservoir(), TimeUnit.Seconds, TimeUnit.Milliseconds);


            //long i = 0;
            //Parallel.ForEach(Enumerable.Range(0, 8), (x) =>
            //{
            //    var scheduler = new ActionScheduler();
            //    var rnd = new Random();
            //    scheduler.Start(TimeSpan.FromMilliseconds(50), () =>
            //    {
            //        timer.Record(rnd.Next() % 1000, TimeUnit.Milliseconds);
            //        Interlocked.Increment(ref i);
            //        if (i % 100 == 0)
            //        {
            //            //          Console.WriteLine(histogram.getEstimatedFootprintInBytes());
            //        }
            //    });
            //});

            Console.WriteLine("press any key to exit");
            Console.ReadKey();
        }
    }
}
