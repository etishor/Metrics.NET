using System;

namespace Metrics.SamplesConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Metric.MachineCounters.RegisterAll();

            Console.WriteLine(Metric.GetAsHumanReadable());

            //SampleMetrics.RunSomeRequests();

            Metric.Reports.PrintConsoleReport(TimeSpan.FromSeconds(1));
            //Metric.Reports.StoreCSVReports(@"c:\temp\reports\", TimeSpan.FromSeconds(1));
            //Metric.Reports.AppendToFile(@"C:\temp\reports\metrics.txt", TimeSpan.FromSeconds(1));

            //var json = JsonConvert.SerializeObject(Metric.GetForSerialization(), Formatting.Indented);

            //File.WriteAllText(@"C:\temp\reports\metrics.json", json);
            //Console.WriteLine(json);

            Console.WriteLine("done");
            Console.ReadKey();
        }
    }
}
