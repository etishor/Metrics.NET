using System;
using Metrics.MetricData;
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
                .WithHttpEndpoint("http://localhost:12345/metrics/")
                .WithAllCounters()
                .WithInternalMetrics()
                .WithReporting(config => config
                    .WithConsoleReport(TimeSpan.FromSeconds(30))
                //.WithCSVReports(@"c:\temp\reports\", TimeSpan.FromSeconds(10))
                //.WithTextFileReport(@"C:\temp\reports\metrics.txt", TimeSpan.FromSeconds(10))
                //.WithGraphite(new Uri("net.udp://localhost:2003"), TimeSpan.FromSeconds(1))
                //.WithInfluxDb("192.168.1.8", 8086, "admin", "admin", "metrics", TimeSpan.FromSeconds(1))
                //.WithElasticSearch("192.168.1.8", 9200, "metrics", TimeSpan.FromSeconds(1))
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

                Metric.Gauge("Errors", () => 1, Unit.None);
                Metric.Gauge("% Percent/Gauge|test", () => 1, Unit.None);
                Metric.Gauge("& AmpGauge", () => 1, Unit.None);
                Metric.Gauge("()[]{} ParantesisGauge", () => 1, Unit.None);
                Metric.Gauge("Gauge With No Value", () => double.NaN, Unit.None);

                //Metric.Gauge("Gauge Resulting in division by zero", () => 5 / 0.0, Unit.None);

                ////Metrics.Samples.FSharp.SampleMetrics.RunSomeRequests();

                HealthChecksSample.RegisterHealthChecks();
                //Metrics.Samples.FSharp.HealthChecksSample.RegisterHealthChecks();

                Console.WriteLine("done setting things up");
                Console.ReadKey();
            }
        }
    }

}
