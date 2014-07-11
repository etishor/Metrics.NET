using System;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Metrics.RollingCsvReporter
{
  public class RuntimeConfiguredCsvRollingLogger
  {
    private readonly int maxFileSize;
    private readonly int rollBackups;

    public RuntimeConfiguredCsvRollingLogger(int maxFileSize = 1000000, int rollBackups = 10)
    {
      this.maxFileSize = maxFileSize;
      this.rollBackups = rollBackups;
    }

    public ILog GetLogger(string loggerName, string csvHeader, string fileName)
    {
      var logger = LogManager.GetLogger(loggerName);

      var myLogger = ((Logger)logger.Logger);

      if (myLogger.Appenders.Count == 0)
      {
        CreateAppenderForLogger(csvHeader, fileName, myLogger);
      }

      return logger;
    }

    private void CreateAppenderForLogger(string csvHeader, string fileName, IAppenderAttachable myLogger)
    {
      var appender = new RollingFileAppender
        {
          File = fileName,
          AppendToFile = true,
          Layout = new PatternLayout("%message%newline")
            {
              Header = csvHeader + Environment.NewLine
            },
          MaxFileSize = maxFileSize,
          MaxSizeRollBackups = rollBackups, 
          RollingStyle = RollingFileAppender.RollingMode.Size
        };

      appender.ActivateOptions();
      myLogger.AddAppender(appender);
      BasicConfigurator.Configure(appender);
    }
  }
  
}