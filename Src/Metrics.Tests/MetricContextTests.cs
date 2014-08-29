using System.Linq;
using FluentAssertions;
using Xunit;

namespace Metrics.Tests
{
    public class MetricContextTests
    {
        [Fact]
        public void ContextCanCreateSubcontext()
        {
            MetricContext context = new MetricContext();

            context.Context("test").Counter("counter", Unit.Requests);

            var counterValue = context.MetricsData.ChildMetrics.SelectMany(c => c.Counters).Single();

            counterValue.Context.Should().Be("test");
            counterValue.Name.Should().Be("counter");
        }
    }
}
