using Metrics.Core;

namespace Metrics.Core
{
    public interface HealthChecksRegistry
    {
        string Name { get; }
        void Register(HealthCheck healthCheck);
        HealthStatus GetStatus();
    }
}
