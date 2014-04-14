using System;
using System.Collections.Generic;
using System.Linq;
using Metrics.Core;

namespace Metrics
{
    /// <summary>
    /// Structure describing the status of executing all the health checks operations.
    /// </summary>
    public struct HealthStatus
    {
        /// <summary>
        /// Flag indicating whether any checks are registered
        /// </summary>
        public readonly bool HasRegisteredChecks;

        /// <summary>
        /// All health checks passed.
        /// </summary>
        public readonly bool IsHealty;

        /// <summary>
        /// Result of each health check operation
        /// </summary>
        public readonly HealthCheck.Result[] Results;

        public HealthStatus(IEnumerable<HealthCheck.Result> results)
        {
            this.Results = results.ToArray();
            this.IsHealty = this.Results.All(r => r.Check.IsHealthy);
            this.HasRegisteredChecks = this.Results.Length > 0;
        }
    }

    /// <summary>
    /// Registry for health checks
    /// </summary>
    public static class HealthChecks
    {
        private static Lazy<HealthChecksRegistry> registry = new Lazy<HealthChecksRegistry>(() => new LocalHealthChecksRegistry(), true);

        /// <summary>
        /// The registry where the checks are registered.
        /// </summary>
        public static HealthChecksRegistry Registry { get { return registry.Value; } }

        /// <summary>
        /// Configure the HealthChecks static class to use a custom HealthChecksRegistry.
        /// </summary>
        /// <remarks>
        /// You must call HealthChecks.ConfigureDefaultRegistry before any other MetricHealthChecks call.
        /// </remarks>
        /// <param name="registry">The custom registry to use for registering health checks.</param>
        public static void ConfigureDefaultRegistry(HealthChecksRegistry registry)
        {
            if (HealthChecks.registry.IsValueCreated)
            {
                throw new InvalidOperationException("HealthChecks registry has already been created. You must call HealthChecks.ConfigureDefaultRegistry before any other HealthChecks call.");
            }
            HealthChecks.registry = new Lazy<HealthChecksRegistry>(() => registry);
        }

        /// <summary>
        /// Registers an action to monitor. If the action throws the health check fails, otherwise is successful.
        /// </summary>
        /// <param name="name">Name of the health check.</param>
        /// <param name="check">Action to execute.</param>
        public static void RegisterHealthCheck(string name, Action check)
        {
            RegisterHealthCheck(new HealthCheck(name, check));
        }

        /// <summary>
        /// Registers an action to monitor. If the action throws the health check fails, 
        /// otherwise is successful and the returned string is used as status message.
        /// </summary>
        /// <param name="name">Name of the health check.</param>
        /// <param name="check">Function to execute.</param>
        public static void RegisterHealthCheck(string name, Func<string> check)
        {
            RegisterHealthCheck(new HealthCheck(name, check));
        }

        /// <summary>
        /// Registers a function to monitor. If the function throws or returns an HealthCheckResult.Unhealthy the check fails,
        /// otherwise the result of the function is used as a status.
        /// </summary>
        /// <param name="name">Name of the health check.</param>
        /// <param name="check">Function to execute</param>
        public static void RegisterHealthCheck(string name, Func<HealthCheckResult> check)
        {
            RegisterHealthCheck(new HealthCheck(name, check));
        }

        /// <summary>
        /// Registers a custom health check.
        /// </summary>
        /// <param name="healthCheck">Custom health check to register.</param>
        public static void RegisterHealthCheck(HealthCheck healthCheck)
        {
            registry.Value.Register(healthCheck);
        }

        /// <summary>
        /// Execute all registered checks and return overall.
        /// </summary>
        /// <returns>Status of the system.</returns>
        public static HealthStatus GetStatus()
        {
            return registry.Value.GetStatus();
        }
    }
}
