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
          .WithHttpEndpoint("http://localhost:1234/")
          .WithErrorHandler(x => Console.WriteLine(x.ToString()))
          .WithAllCounters()
          .WithReporting(config => config
              .WithConsoleReport(TimeSpan.FromSeconds(30))
              .WithCSVReports(@"c:\temp\reports\", TimeSpan.FromSeconds(10))
              .WithTextFileReport(@"C:\temp\reports\metrics.txt", TimeSpan.FromSeconds(10))
          );

      SampleMetrics.RunSomeRequests();
      //Metrics.Samples.FSharp.SampleMetrics.RunSomeRequests();

      HealthChecksSample.RegisterHealthChecks();
      //Metrics.Samples.FSharp.HealthChecksSample.RegisterHealthChecks();

      Console.WriteLine("done setting things up");
      Console.ReadKey();
    }
  }
}
