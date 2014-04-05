using System;
using FluentAssertions;
using Metrics.Visualization;
using Xunit;

namespace Metrics.Tests.Visualization
{
    public class FlotVisualizationTests
    {
        [Fact]
        public void CanReadAppFromResource()
        {
            FlotWebApp.GetFlotApp(new Uri("http://localhost/")).Should().NotBeEmpty();
            FlotWebApp.GetFlotApp(new Uri("http://xxx/")).Should().Contain("http://xxx/");
        }
    }
}
