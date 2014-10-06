﻿using System;
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
                .WithHttpEndpoint("http://localhost:1234/")
                .WithAllCounters()
                .WithInternalMetrics()
                .WithReporting(config => config
                    //.WithNLogCSVReports(TimeSpan.FromSeconds(5))
                    //.WithNLogTextReports(TimeSpan.FromSeconds(5))
                    //.WithReporter("CSV Reports", () => new CSVReporter(new RollingCSVFileAppender(@"c:\temp\csv")), TimeSpan.FromSeconds(10))
                    .WithConsoleReport(TimeSpan.FromSeconds(30))
                //.WithCSVReports(@"c:\temp\reports\", TimeSpan.FromSeconds(10))
                //.WithTextFileReport(@"C:\temp\reports\metrics.txt", TimeSpan.FromSeconds(10))
                );

            using (var scheduler = new ActionScheduler())
            {
                SampleMetrics.RunSomeRequests();
                scheduler.Start(TimeSpan.FromMilliseconds(500), () => SampleMetrics.RunSomeRequests());

                //Metrics.Samples.FSharp.SampleMetrics.RunSomeRequests();

                HealthChecksSample.RegisterHealthChecks();
                //Metrics.Samples.FSharp.HealthChecksSample.RegisterHealthChecks();

                Console.WriteLine("done setting things up");
                Console.ReadKey();
            }
        }
    }

}
