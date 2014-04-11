using System;
using Metrics.Samples;
using Metrics.Visualization;

namespace Metrics.SamplesConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Metric.MachineCounters.RegisterAll();

            SampleMetrics.RunSomeRequests();

            //Metric.Reports.PrintConsoleReport(TimeSpan.FromSeconds(1));

            //var json = new RegistrySerializer().ValuesAsJson(Metric.Registry);
            //var x = JsonConvert.DeserializeObject<dynamic>(json);
            //Console.WriteLine(json);

            //Metric.Reports.StoreCSVReports(@"c:\temp\reports\", TimeSpan.FromSeconds(1));
            //Metric.Reports.AppendToFile(@"C:\temp\reports\metrics.txt", TimeSpan.FromSeconds(1));

            using (MetricsHttpListener http = new MetricsHttpListener("http://localhost:1234/"))
            {
                http.Start();
                Console.WriteLine("done");
                Console.ReadKey();
                http.Stop();
            }

            //Console.WriteLine("done");
            //Console.ReadKey();
        }
    }
}
