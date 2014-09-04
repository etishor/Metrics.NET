using Metrics.Utils;

namespace Metrics.Json
{
    public static class JsonMetrics
    {
        public static string Serialize(MetricsData metricsData)
        {
            return new JsonBuilder().BuildJson(metricsData, Clock.Default);
        }
    }
}
