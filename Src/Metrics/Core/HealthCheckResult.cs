using System;

namespace Metrics.Core
{
    public struct HealthCheckResult
    {
        public readonly string Name;
        public readonly bool IsHealthy;
        public readonly string Message;

        private HealthCheckResult(string name, bool isHealthy, string message)
        {
            this.Name = name;
            this.IsHealthy = isHealthy;
            this.Message = message;
        }

        public static HealthCheckResult Healthy(string name, string message, params object[] values)
        {
            return new HealthCheckResult(name, true, string.Format(message, values));
        }

        public static HealthCheckResult Unhealthy(string name, string message, params object[] values)
        {
            return new HealthCheckResult(name, false, string.Format(message, values));
        }

        public static HealthCheckResult Unhealthy(string name, Exception x)
        {
            return HealthCheckResult.Unhealthy(name, x.Message);
        }
    }
}
