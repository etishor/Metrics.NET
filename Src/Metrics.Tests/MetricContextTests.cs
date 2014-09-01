using System.Linq;
using FluentAssertions;
using Xunit;

namespace Metrics.Tests
{
    public class MetricContextTests
    {
        [Fact]
        public void ContextEmptyChildContextIsSameContext()
        {
            MetricContext context = new MetricContext();

            var child = context.Context(string.Empty);

            object.ReferenceEquals(context, child).Should().BeTrue();

            child = context.Context(null);

            object.ReferenceEquals(context, child).Should().BeTrue();
        }

        [Fact]
        public void ContextCanCreateSubcontext()
        {
            MetricContext context = new MetricContext();

            context.Context("test").Counter("counter", Unit.Requests);

            var counterValue = context.MetricsData.ChildMetrics.SelectMany(c => c.Counters).Single();

            counterValue.Context.Should().Be("test");
            counterValue.Name.Should().Be("counter");
        }

        [Fact]
        public void ContextMetricsArePresentInMetricsData()
        {
            MetricContext context = new MetricContext();

            var counter = context.Counter("test", Unit.Requests);

            counter.Increment();

            var counterValue = context.MetricsData.Counters.Single();

            counterValue.Name.Should().Be("test");
            counterValue.Unit.Should().Be(Unit.Requests);
            counterValue.Value.Should().Be(1);
        }
    }
}
