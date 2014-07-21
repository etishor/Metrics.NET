using Metrics.Samples;
using Metrics.Utils;
using Microsoft.Owin.Hosting;
using System;
using System.Diagnostics;

namespace Owin.Sample
{
    public class Program
    {
        static void Main(string[] args)
        {
            const string url = "http://localhost:1234/";

            using (var scheduler = new ActionScheduler())
            {
                using (WebApp.Start<Startup>(url))
                {
                    Console.WriteLine("Owin Running at {0}", url);
                    Console.WriteLine("Press any key to exit");
                    Process.Start(string.Format("{0}metrics", url));

                    SampleMetrics.RunSomeRequests();

                    scheduler.Start(TimeSpan.FromMilliseconds(500), () => SampleMetrics.RunSomeRequests());

                    Console.ReadKey();
                }
            }
        }
    }
}
