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

            Metric.Reports.StoreCSVReports(@"c:\temp\reports\", TimeSpan.FromSeconds(1));
            Metric.Reports.AppendToFile(@"C:\temp\reports\metrics.txt", TimeSpan.FromSeconds(1));

            Metric.Reports.StartHttpListener("http://localhost:1234/");

            Console.WriteLine("done");
            Console.ReadKey();
        }
    }
}
