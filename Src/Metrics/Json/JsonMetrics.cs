using System.Linq;
using Metrics.Utils;

namespace Metrics.Json
{
    public static class JsonMetrics
    {
        public static string Serialize(MetricsData metricsData)
        {
            metricsData.Gauges.GroupBy(g => g.Context)
                .Select(g => new
                {
                    Context = g.Key,
                    Gauges = g.AsEnumerable()
                });

            return new JsonBuilder()
                .AddTimestamp(Clock.Default)
                .Add(metricsData.Gauges)
                .Add(metricsData.Counters)
                .Add(metricsData.Meters)
                .Add(metricsData.Histograms)
                .Add(metricsData.Timers)
                .GetJson();
        }
    }
}
