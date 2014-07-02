using System;
using System.Collections.Generic;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Metrics.Reporters;

namespace Metrics.SampleReporter
{
    /// <summary>
    /// Copied from
    /// https://github.com/nkot/Metrics.NET/blob/master/Src/Reporters/Metrics.RollingCsvReporter/RollingCsvFileAppender.cs
    /// </summary>
    public class RollingCSVFileAppender : CSVFileAppender
    {
        private readonly string directory;
        private readonly Func<string, string, string, ILog> getLogger;

        public RollingCSVFileAppender(string directory, string delimiter = ",", int maxFileSize = 1000000, int rollBackups = 10)
            : this(directory, new RuntimeConfiguredCsvRollingLogger(maxFileSize, rollBackups).GetLogger, delimiter)
        { }

        public RollingCSVFileAppender(string directory, Func<string, string, string, ILog> getLogger, string delimiter)
            : base(directory, delimiter)
        {
            this.directory = directory;
            this.getLogger = getLogger;
        }

        public override void AppendLine(DateTime timestamp, string metricType, string metricName, IEnumerable<CSVReporter.Value> values)
        {
            var fileName = FormatFileName(directory, metricName, metricType);

            var logger = getLogger(metricName + metricType, GetHeader(values), fileName);

            logger.Info(GetValues(timestamp, values));
        }
    }

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
