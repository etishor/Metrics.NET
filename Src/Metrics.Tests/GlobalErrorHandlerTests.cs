using System;
using System.Threading;
using FluentAssertions;
using Xunit;

namespace Metrics.Tests
{
    public class GlobalErrorHandlerTests
    {
        [Fact]
        public void GlobalErrorHandlerIsCalledWithException()
        {
            Exception exception = null;

            Metric.ErrorHandler = (x) => exception = x;
            Metric.Gauge("error", () => { throw new Exception(); }, Unit.None);
            Metric.Reports.PrintConsoleReport(TimeSpan.FromMilliseconds(1));

            Thread.Sleep(100);

            exception.Should().NotBeNull();

            Metric.Reports.StopAndClearAllReports();
        }
    }
}
