using System;
using System.IO;
using System.Linq;
using System.Text;
using Metrics.Log4Net.Layout;
using Xunit;
using Xunit.Extensions;
using log4net.Core;

namespace Metrics.Log4Net.Tests
{
    public class CsvMetricLayoutTest
    {
        [Theory]
        [InlineData(typeof(CsvHistogramLayout))]
        [InlineData(typeof(CsvGaugeLayout))]
        [InlineData(typeof(CsvMeterLayout))]
        [InlineData(typeof(CsvTimerLayout))]
        [InlineData(typeof(CsvCounterLayout))]
        public void HeaderContainsAllColumnsSplittableByDelimiter(Type value)
        {
            var sut = (CsvLayout)Activator.CreateInstance(value);
            Assert.IsType(value, sut);

            var header = sut.Header.Split(new[] { CsvDelimiter.Value, Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            Assert.Equal(header, sut.Columns.Select(x => x.ColumnName).ToArray());
        }

        [Theory]
        [InlineData(typeof(CsvHistogramLayout))]
        [InlineData(typeof(CsvGaugeLayout))]
        [InlineData(typeof(CsvMeterLayout))]
        [InlineData(typeof(CsvTimerLayout))]
        [InlineData(typeof(CsvCounterLayout))]
        public void TheOutputContainDataForMatchingColumn(Type value)
        {
            var sut = (CsvLayout)Activator.CreateInstance(value);
            Assert.IsType(value, sut);
            Assert.Contains("Date", sut.Columns.Select(x => x.ColumnName));

            var loggingEvent = new LoggingEvent(new LoggingEventData());
            loggingEvent.Properties["Date"] = "2014-07-10T11:17Z";

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                sut.Format(writer, loggingEvent);
                writer.Flush();

                var actual = Encoding.UTF8.GetString(stream.ToArray());
                Assert.Contains("2014-07-10T11:17Z", actual);
            }
        }

        [Theory]
        [InlineData(typeof(CsvHistogramLayout))]
        [InlineData(typeof(CsvGaugeLayout))]
        [InlineData(typeof(CsvMeterLayout))]
        [InlineData(typeof(CsvTimerLayout))]
        [InlineData(typeof(CsvCounterLayout))]
        public void TheOutputWillNotHaveDataForNonMatchingColumn(Type value)
        {
            var sut = (CsvLayout)Activator.CreateInstance(value);
            Assert.IsType(value, sut);
            Assert.DoesNotContain("IDoNotHaveThisColumn", sut.Columns.Select(x => x.ColumnName));

            var loggingEvent = new LoggingEvent(new LoggingEventData());
            loggingEvent.Properties["IDoNotHaveThisColumn"] = "I Should Not Be Here";

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                sut.Format(writer, loggingEvent);
                writer.Flush();

                var actual = Encoding.UTF8.GetString(stream.ToArray());
                Assert.DoesNotContain("I Should Not Be Here", actual);
            }
        }

        [Theory]
        [InlineData(typeof(CsvHistogramLayout))]
        [InlineData(typeof(CsvGaugeLayout))]
        [InlineData(typeof(CsvMeterLayout))]
        [InlineData(typeof(CsvTimerLayout))]
        [InlineData(typeof(CsvCounterLayout))]
        public void TheOutputWillContainOneDelimiterPerColumn(Type value)
        {
            var sut = (CsvLayout)Activator.CreateInstance(value);
            Assert.IsType(value, sut);

            var loggingEvent = new LoggingEvent(new LoggingEventData());

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                sut.Format(writer, loggingEvent);
                writer.Flush();

                var actual = Encoding.UTF8.GetString(stream.ToArray());
                Assert.NotEmpty(actual);
                var numberOfDelimitersInOutput = actual.Split(new[] { CsvDelimiter.Value }, StringSplitOptions.None).Length - 1;
                Assert.Equal(sut.Columns.Count, numberOfDelimitersInOutput);
            }
        }
    }
}
