using System.Collections.Concurrent;
using System.Linq;

namespace Metrics.Core
{
    public class LocalHealthChecksRegistry : HealthChecksRegistry
    {
        private const string DefaultName = "Health Check";

        private readonly ConcurrentDictionary<string, HealthCheck> checks = new ConcurrentDictionary<string, HealthCheck>();

        public LocalHealthChecksRegistry()
            : this(DefaultName)
        { }

        public LocalHealthChecksRegistry(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }

        public void Register(HealthCheck healthCheck)
        {
            checks.AddOrUpdate(healthCheck.Name, healthCheck, (n, c) => healthCheck);
        }

        public HealthStatus GetStatus()
        {
            var results = checks.Values.Select(v => v.Execute()).OrderBy(r => r.Name);
            return new HealthStatus(results);
        }
    }
}
