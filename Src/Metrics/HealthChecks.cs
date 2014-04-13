using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Metrics.Core;

namespace Metrics
{
    public struct HealthStatus
    {
        public readonly bool IsHealty;
        public readonly HealthCheckResult[] Results;

        public HealthStatus(bool isHealthy, IEnumerable<HealthCheckResult> results)
        {
            this.IsHealty = isHealthy;
            this.Results = results.ToArray();
        }
    }

    public class HealthChecks
    {
        private readonly ConcurrentDictionary<string, HealthCheck> checks = new ConcurrentDictionary<string, HealthCheck>();

        public void RegisterHealthCheck(string name, Action check)
        {
            RegisterHealthCheck(new HealthCheck(name, check));
        }

        public void RegisterHealthCheck(string name, Func<string> check)
        {
            RegisterHealthCheck(new HealthCheck(name, check));
        }

        public void RegisterHealthCheck(string name, Func<HealthCheckResult> check)
        {
            RegisterHealthCheck(new HealthCheck(name, check));
        }

        public void RegisterHealthCheck(HealthCheck check)
        {
            this.checks.AddOrUpdate(check.Name, check, (n, c) => check);
        }

        public IEnumerable<HealthCheckResult> GetStatus()
        {
            return this.checks.Values.Select(v => v.Execute()).OrderBy(r => r.Name);
        }
    }
}
