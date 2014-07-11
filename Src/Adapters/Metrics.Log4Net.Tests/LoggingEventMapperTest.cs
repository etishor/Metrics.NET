using System;
using Metrics.Log4Net.Appenders;
using Ploeh.AutoFixture.Xunit;
using Xunit;
using Xunit.Extensions;
using log4net.Core;

namespace Metrics.Log4Net.Tests
{
    public class LoggingEventMapperTest
    {
        [Theory, AutoData]
        public void LevelIsInfo(string loggerName, DateTime dateTime, MetricsData metricsData, LoggingEventMapper sut)
        {
            var actual = sut.MapToLoggingEvent(loggerName, dateTime, metricsData);
            Assert.Equal(Level.Info, actual.Level);
        }

        [Theory, AutoData]
        public void LoggerNameIsCopied(string loggerName, DateTime dateTime, MetricsData metricsData, LoggingEventMapper sut)
        {
            var actual = sut.MapToLoggingEvent(loggerName, dateTime, metricsData);
            Assert.Equal(loggerName, actual.LoggerName);
        }

        [Theory, AutoData]
        public void DateTimeIsCreated(string loggerName, DateTime dateTime, MetricsData metricsData, LoggingEventMapper sut)
        {
            var actual = sut.MapToLoggingEvent(loggerName, dateTime, metricsData);
            Assert.Equal(dateTime.ToString("u"), actual.Properties["Date"]);
        }

        [Theory, AutoData]
        public void TicksIsCreated(string loggerName, DateTime dateTime, MetricsData metricsData, LoggingEventMapper sut)
        {
            var actual = sut.MapToLoggingEvent(loggerName, dateTime, metricsData);
            Assert.Equal(dateTime.Ticks.ToString("D"), actual.Properties["Ticks"]);
        }

        [Theory, AutoData]
        public void MetricsDataPropertiesAreCopied(string loggerName, DateTime dateTime, MetricsData metricsData, LoggingEventMapper sut)
        {
            var actual = sut.MapToLoggingEvent(loggerName, dateTime, metricsData);

            Assert.Equal(metricsData.MetricType, actual.Properties["MetricType"]);
            Assert.Equal(metricsData.MetricName, actual.Properties["MetricName"]);

            foreach (var value in metricsData.Values)
            {
                Assert.Equal(value.FormattedValue, actual.Properties[value.Name]);
            }
        }
    }
}