
using Metrics.Json;
namespace Metrics.Reporters
{
    public static class HealthCheckSerializer
    {
        public static string Serialize(HealthStatus status)
        {
            return new JsonFormatter().AddObject(status).GetJson();
        }
    }
}
