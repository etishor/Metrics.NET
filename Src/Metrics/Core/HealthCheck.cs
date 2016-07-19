using System;

namespace Metrics.Core
{
    public class HealthCheck
    {
        public struct Result
        {
            public readonly string Name;
            public readonly HealthCheckResult Check;

            public Result(string name, HealthCheckResult check)
            {
                this.Name = name;
                this.Check = check;
            }
        }

        internal Func<HealthCheckResult> CheckFunc { get; set; }

        protected HealthCheck(string name)
            : this(name, () => { })
        { }

        public HealthCheck(string name, Action check)
            : this(name, () => { check(); return string.Empty; })
        { }

        public HealthCheck(string name, Func<string> check)
            : this(name, () => HealthCheckResult.Healthy(check()))
        { }

        public HealthCheck(string name, Func<HealthCheckResult> check)
        {
            this.Name = name;
            this.CheckFunc = check;
        }

        public string Name { get; }

        protected virtual HealthCheckResult Check()
        {
            return this.CheckFunc();
        }

        public Result Execute()
        {
            try
            {
                return new Result(this.Name, this.Check());
            }
            catch (Exception x)
            {
                return new Result(this.Name, HealthCheckResult.Unhealthy(x));
            }
        }
    }
}
