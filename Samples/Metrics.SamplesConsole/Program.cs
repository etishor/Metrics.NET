using System;
using Metrics.Samples;

namespace Metrics.SamplesConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Metric.MachineCounters.RegisterAll();

            SampleMetrics.RunSomeRequests();
            //Metrics.Samples.FSharp.SampleMetrics.RunSomeRequests();

            HealthChecksSample.RegisterHealthChecks();
            //Metrics.Samples.FSharp.HealthChecksSample.RegisterHealthChecks();

            Metric.Reports.PrintConsoleReport(TimeSpan.FromSeconds(10));
            Metric.Reports.StoreCSVReports(@"c:\temp\reports\", TimeSpan.FromSeconds(10));
            Metric.Reports.AppendToFile(@"C:\temp\reports\metrics.txt", TimeSpan.FromSeconds(10));

            Metric.Reports.StartHttpListener("http://localhost:1234/");

            Console.WriteLine("done setting things up");
            Console.ReadKey();
        }
    }
}
