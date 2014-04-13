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

        public HealthStatus(IEnumerable<HealthCheckResult> results)
        {
            this.Results = results.ToArray();
            this.IsHealty = this.Results.All(r => r.IsHealthy);
        }
    }

    public static class HealthChecks
    {
        private static readonly ConcurrentDictionary<string, HealthCheck> checks = new ConcurrentDictionary<string, HealthCheck>();

        public static void RegisterHealthCheck(string name, Action check)
        {
            RegisterHealthCheck(new HealthCheck(name, check));
        }

        public static void RegisterHealthCheck(string name, Func<string> check)
        {
            RegisterHealthCheck(new HealthCheck(name, check));
        }

        public static void RegisterHealthCheck(string name, Func<HealthCheckResult> check)
        {
            RegisterHealthCheck(new HealthCheck(name, check));
        }

        public static void RegisterHealthCheck(HealthCheck check)
        {
            checks.AddOrUpdate(check.Name, check, (n, c) => check);
        }

        public static HealthStatus GetStatus()
        {
            var results = checks.Values.Select(v => v.Execute()).OrderBy(r => r.Name);
            return new HealthStatus(results);
        }
    }
}
