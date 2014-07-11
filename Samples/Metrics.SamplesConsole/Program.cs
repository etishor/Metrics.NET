using System;
using System.IO;
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
                    .WithNLogCSVReports(TimeSpan.FromSeconds(5))
                    .WithNLogTextReports(TimeSpan.FromSeconds(5))
                    
                    .WithCustomMetricsLog4NetCsvDelimiter()
                    .WithDefaultMetricsLog4NetConfigFile(@".\Metrics.Log4Net.Logs\") //or log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo("Log4Net.config")); //log4net.Util.LogLog.InternalDebugging = true;
                    .WithLog4NetCSVReports(TimeSpan.FromSeconds(5))
                    .WithLog4NetTextReports(TimeSpan.FromSeconds(5))
                    
                    //.WithReporter("CSV Reports", () => new CSVReporter(new RollingCSVFileAppender(@"c:\temp\csv")), TimeSpan.FromSeconds(10))
                    .WithConsoleReport(TimeSpan.FromSeconds(30))
                //.WithCSVReports(@"c:\temp\reports\", TimeSpan.FromSeconds(10))
                //.WithTextFileReport(@"C:\temp\reports\metrics.txt", TimeSpan.FromSeconds(10))
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
