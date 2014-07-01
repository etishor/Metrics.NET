using System;
using Metrics.Reports;
using Metrics.RollingCsvReporter;
using Metrics.Samples;

namespace Metrics.SamplesConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //Metric.Config.CompletelyDisableMetrics();

            Metric.Config
                .WithHttpEndpoint("http://localhost:1234/")
                .WithErrorHandler(x => Console.WriteLine(x.ToString()))
                .WithAllCounters()
                .WithReporting(config => config
                    .WithConsoleReport(TimeSpan.FromSeconds(30))
                    //.WithCSVReports(@"c:\temp\reports\", TimeSpan.FromSeconds(10))
                    //.WithTextFileReport(@"C:\temp\reports\metrics.txt", TimeSpan.FromSeconds(10))
                    //.WithScheduledReporter(new RollingCsvScheduledReporter().Create(@"c:\temp\rolling\", TimeSpan.FromSeconds(5)))
                    .WithRollingCSVReports(@"c:\temp\rolling\", TimeSpan.FromSeconds(5), 1000, 1, ";")
                );

            SampleMetrics.RunSomeRequests();
            //Metrics.Samples.FSharp.SampleMetrics.RunSomeRequests();

            //HealthChecksSample.RegisterHealthChecks();
            //Metrics.Samples.FSharp.HealthChecksSample.RegisterHealthChecks();

            Console.WriteLine("done setting things up");
            Console.ReadKey();
        }
    }
}
