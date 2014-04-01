
using System;
namespace Metrics.PerfCounters
{
    public abstract class BaseCounterRegristry
    {
        protected const string TotalInstance = "_Total";
        protected const string GlobalInstance = "_Global_";

        private readonly string prefix;
        private readonly MetricsRegistry registry;

        public BaseCounterRegristry(MetricsRegistry registry, string prefix)
        {
            this.registry = registry;
            this.prefix = prefix;
        }

        protected void Register(string name, Func<Gauge> gauge, Unit unit)
        {
            this.registry.Gauge(prefix + "." + name, gauge, unit);
        }

        protected void Register(string name, Func<string> value)
        {
            Register(name, value, Unit.None);
        }

        protected void Register(string name, Func<float> value, Unit unit)
        {
            Register(name, () => value().ToString("F"), unit);
        }

        protected void Register(string name, Func<string> value, Unit unit)
        {
            this.registry.Gauge(prefix + "." + name, value, unit);
        }
    }
}
