
using System;
using Metrics.Core;

namespace Metrics.PerfCounters
{
    public abstract class BaseCounterRegristry
    {
        protected const string TotalInstance = "_Total";
        protected const string GlobalInstance = "_Global_";

        private readonly string prefix;
        private readonly MetricsRegistry registry;

        protected BaseCounterRegristry(MetricsRegistry registry, string prefix)
        {
            this.registry = registry;
            this.prefix = prefix;
        }

        protected void Register(string name, Func<GaugeMetric> gauge, Unit unit)
        {
            this.registry.Gauge(prefix + "." + name, gauge, unit);
        }
    }
}
