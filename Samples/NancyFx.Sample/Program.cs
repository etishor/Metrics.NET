using System;
using System.Diagnostics;
using Metrics.Samples;
using Nancy.Hosting.Self;
using Newtonsoft.Json;

namespace NancyFx.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings { Formatting = Formatting.Indented };

            using (var host = new NancyHost(new Uri("http://localhost:1234")))
            {
                host.Start();
                Console.WriteLine("Nancy Running at http://localhost:1234");
                Console.WriteLine("Press any key to exit");
                Process.Start("http://localhost:1234/metrics/");

                SampleMetrics.RunSomeRequests();

                Console.ReadKey();
            }
        }
    }
}
