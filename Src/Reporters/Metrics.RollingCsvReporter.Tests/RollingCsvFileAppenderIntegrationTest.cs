using System;
using System.Collections.Generic;
using System.IO;
using Metrics.Reporters;
using Xunit;
using log4net;

namespace Metrics.RollingCsvReporter.Tests
{
    public class RollingCsvFileAppenderIntegration
    {

      [Fact]
      public void CanLogToFile()
      {
        using (var tempDirectory = new TemporaryDirectoryFixture())
        {
          var sut = new RollingCsvFileAppender(tempDirectory.DirectoryPath, new RuntimeConfiguredCsvRollingLogger().GetLogger, ";");
          sut.AppendLine(DateTime.Now, "Timer", "Metric", new List<CSVReporter.Value> {new CSVReporter.Value("Test", 1)});

          LogManager.ShutdownRepository();

          var expectedFileName = Path.Combine(tempDirectory.DirectoryPath, "Metric.Timer.csv");
          Assert.True(File.Exists(expectedFileName));
        }
      }

      [Fact]
      public void LogFilesRoll()
      {
        using (var tempDirectory = new TemporaryDirectoryFixture())
        {
          var sut = new RollingCsvFileAppender(tempDirectory.DirectoryPath, new RuntimeConfiguredCsvRollingLogger(maxFileSize: 100, rollBackups: 1).GetLogger, ";");
          for (int i = 0; i < 100; i++)
          {
            sut.AppendLine(DateTime.Now, "Timer", "Metric",
                           new List<CSVReporter.Value> {new CSVReporter.Value("Test", 1)});
          }

          LogManager.ShutdownRepository();

          var rolledFileName = Path.Combine(tempDirectory.DirectoryPath, "Metric.Timer.csv.1");
          Assert.True(File.Exists(rolledFileName));
        }
      }
    }
}

