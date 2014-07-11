using System;
using System.Collections.Generic;
using System.Linq;
using Metrics.Log4Net.Appenders;
using Metrics.Reporters;
using Moq;
using Ploeh.AutoFixture.Xunit;
using Xunit.Extensions;
using log4net.Core;

namespace Metrics.Log4Net.Tests
{
    public class Log4NetCSVAppenderTest
    {
        [Theory, AutoMoqData]
        public void DoNotLogIfLevelLessThanInfo([Frozen] Mock<ILoggerProvider> loggerProvider, [Frozen] Mock<ILogger> logger, Log4NetCSVAppender sut)
        {
            loggerProvider.Setup(x => x.GetLogger(It.IsAny<MetricsData>())).Returns(logger.Object);

            logger.Setup(x => x.IsEnabledFor(Level.Info)).Returns(false);

            sut.AppendLine(DateTime.Now, "MetricType", "MetricName", new List<CSVReporter.Value>());

            logger.Verify(x => x.Log(It.IsAny<LoggingEvent>()), Times.Never());
        }

        [Theory, AutoMoqData]
        public void WillLogIfLevelIsInfo([Frozen] Mock<ILoggerProvider> loggerProvider, [Frozen] Mock<ILogger> logger, [Frozen] Mock<ILoggingEventMapper> loggingEventMapper, Log4NetCSVAppender sut)
        {
            loggerProvider.Setup(x => x.GetLogger(It.IsAny<MetricsData>())).Returns(logger.Object);

            logger.Setup(x => x.IsEnabledFor(Level.Info)).Returns(true);
            loggingEventMapper.Setup(x => x.MapToLoggingEvent(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<MetricsData>())).Returns(new LoggingEvent(new LoggingEventData()));

            sut.AppendLine(DateTime.Now, "MetricType", "MetricName", new List<CSVReporter.Value>());

            logger.Verify(x => x.Log(It.IsAny<LoggingEvent>()));
        }

        [Theory, AutoMoqData]
        public void LoggingEventPropertiesAreFilled([Frozen] Mock<ILoggerProvider> loggerProvider, [Frozen] Mock<ILogger> logger, [Frozen] Mock<ILoggingEventMapper> loggingEventMapper, Log4NetCSVAppender sut)
        {
            loggerProvider.Setup(x => x.GetLogger(It.IsAny<MetricsData>())).Returns(logger.Object);

            logger.Setup(x => x.IsEnabledFor(Level.Info)).Returns(true);

            var actualLoggingEvent = new LoggingEvent(new LoggingEventData());
            loggingEventMapper.Setup(x => x.MapToLoggingEvent(logger.Object.Name, It.IsAny<DateTime>(), It.IsAny<MetricsData>())).Returns(actualLoggingEvent);

            sut.AppendLine(DateTime.Now, "Timer", "SomeTimerMetric", new List<CSVReporter.Value> { new CSVReporter.Value("Count", 1) });

            logger.Verify(x => x.Log(actualLoggingEvent));
        }
    }
}