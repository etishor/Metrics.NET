using System;

namespace Metrics.Core
{
    public sealed class HealthCheck
    {
        private readonly Func<HealthCheckResult> check;

        public HealthCheck(string name, Action check)
            : this(name, () => { check(); return string.Empty; })
        { }

        public HealthCheck(string name, Func<string> check)
            : this(name, MapFunction(name, check))
        { }

        public HealthCheck(string name, Func<HealthCheckResult> check)
        {
            this.Name = name;
            this.check = check;
        }

        public string Name { get; private set; }

        private static Func<HealthCheckResult> MapFunction(string name, Func<string> check)
        {
            return () =>
            {
                try
                {
                    var result = check();
                    return HealthCheckResult.Healthy(name, result);
                }
                catch (Exception x)
                {
                    return HealthCheckResult.Unhealthy(name, x);
                }
            };
        }

        public HealthCheckResult Execute()
        {
            try
            {
                return this.check();
            }
            catch (Exception x)
            {
                return HealthCheckResult.Unhealthy(this.Name, x);
            }
        }
    }
}
