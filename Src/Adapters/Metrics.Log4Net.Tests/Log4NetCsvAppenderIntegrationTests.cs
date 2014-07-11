using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Metrics.Log4Net.Appenders;
using Metrics.Reporters;
using Ploeh.AutoFixture.Xunit;
using Xunit;
using Xunit.Extensions;
using log4net;

namespace Metrics.Log4Net.Tests
{
    [Trait("Category", "Integration")]
    public class Log4NetCsvAppenderIntegrationTests
    {
        public Log4NetCsvAppenderIntegrationTests()
        {
            log4net.Util.LogLog.InternalDebugging = true;
        }

        [Theory(Skip = "Performance test. Run locally")]
        [InlineAutoData("Timer")]
        public void DefaultLogConfigurationWillRollFiles(string metricType, string metricName)
        {
            using (var defaultLog4NetConfiguration = new RealLog4NetConfigurationFixture())
            {
                var sut = new Log4NetCSVAppender(new RealLog4NetLoggerProvider(), new LoggingEventMapper());
                for (int i = 0; i < 100000; i++) // > 10 Mb
                {
                    sut.AppendLine(DateTime.Now, metricType, metricName, new List<CSVReporter.Value>());
                }
                LogManager.ShutdownRepository();

                var rolledFileName = Path.Combine(defaultLog4NetConfiguration.LogsDirectory, string.Format("Metrics.CSV.{0}.csv.1", metricType));
                Assert.True(File.Exists(rolledFileName));
                Console.WriteLine(File.ReadAllText(rolledFileName));
                Assert.True(File.ReadAllLines(rolledFileName).Count() >= 2, "log file should have at least 2 lines, one for header and one for data");
            }
        }
    }
}

