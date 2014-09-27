using CsQuery;
using FluentAssertions;
using Metrics.Visualization;
using Xunit;

namespace Metrics.Tests.Visualization
{
    public class FlotVisualizationTests
    {
        [Fact]
        public void FlotVisualization_CanReadAppFromResource()
        {
            var html = FlotWebApp.GetFlotApp();
            html.Should().NotBeEmpty();
            Assert.DoesNotThrow(() => CQ.CreateDocument(html));
        }
    }
}
