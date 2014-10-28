using System;
using Metrics.Samples;
using Metrics.Utils;

namespace Metrics.SamplesConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //Metric.CompletelyDisableMetrics();

            Metric.Config
                .WithHttpEndpoint("http://localhost:1234/metrics/")
                .WithAllCounters()
                .WithInternalMetrics()
                .WithReporting(config => config
                    .WithConsoleReport(TimeSpan.FromSeconds(30))
                    //.WithCSVReports(@"c:\temp\reports\", TimeSpan.FromSeconds(10))
                    //.WithTextFileReport(@"C:\temp\reports\metrics.txt", TimeSpan.FromSeconds(10))
                );

            using (var scheduler = new ActionScheduler())
            {
                SampleMetrics.RunSomeRequests();

                scheduler.Start(TimeSpan.FromMilliseconds(500), () =>
                    {
                        SetCounterSample.RunSomeRequests();
                        SetMeterSample.RunSomeRequests();
                        UserValueHistogramSample.RunSomeRequests();
                        UserValueTimerSample.RunSomeRequests();
                        SampleMetrics.RunSomeRequests();
                    });

                Metric.Gauge("Gauge With No Value", () => double.NaN, Unit.None);
                Metric.Gauge("Gauge Resulting in division by zero", () => 5 / 0.0, Unit.None);

                ////Metrics.Samples.FSharp.SampleMetrics.RunSomeRequests();

                HealthChecksSample.RegisterHealthChecks();
                //Metrics.Samples.FSharp.HealthChecksSample.RegisterHealthChecks();

                Console.WriteLine("done setting things up");
                Console.ReadKey();
            }
        }
    }

}
