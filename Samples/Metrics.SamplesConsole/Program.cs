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
                    r.PrintConsoleReport(TimeSpan.FromSeconds(30));
                    r.StoreCSVReports(@"c:\temp\reports\", TimeSpan.FromSeconds(10));
                    r.AppendToFile(@"C:\temp\reports\metrics.txt", TimeSpan.FromSeconds(10));
                    r.StartHttpListener("http://localhost:1234/");
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
