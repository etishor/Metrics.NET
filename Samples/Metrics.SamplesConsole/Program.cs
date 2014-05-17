using System;
using Metrics.Samples;

namespace Metrics.SamplesConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //Metric.Config.CompletelyDisableMetrics();

            Metric.Config
                .WithPerformanceCounters(c => c.RegisterAll())
                .WithReporting(r =>
                {
                    r.StartConsoleReport(TimeSpan.FromSeconds(30));
                    r.StartCSVReports(@"c:\temp\reports\", TimeSpan.FromSeconds(10));
                    r.StartTextFileReport(@"C:\temp\reports\metrics.txt", TimeSpan.FromSeconds(10));
                });
            
            SampleMetrics.RunSomeRequests();
            //Metrics.Samples.FSharp.SampleMetrics.RunSomeRequests();

            HealthChecksSample.RegisterHealthChecks();
            //Metrics.Samples.FSharp.HealthChecksSample.RegisterHealthChecks();

            Console.WriteLine("done setting things up");
            Console.ReadKey();
        }
    }
}
