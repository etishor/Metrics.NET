using Metrics.Core;

namespace Metrics
{
    public interface HealthChecksRegistry
    {
        string Name { get; }
        void Register(HealthCheck healthCheck);
        HealthStatus GetStatus();
    }
}
