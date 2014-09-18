
using Metrics.Core;
namespace Metrics
{
    public sealed class DefaultMetricsContext : BaseMetricsContext
    {
        public DefaultMetricsContext()
            : this(string.Empty) { }

        public DefaultMetricsContext(string context)
            : this(context, new DefaultMetricsRegistry(), new DefaultMetricsBuilder())
        { }

        public DefaultMetricsContext(string context, MetricsRegistry registry, MetricsBuilder metricsBuilder)
            : base(context, registry, metricsBuilder)
        {
        }

        protected override MetricsContext CreateChildContextInstance(string contextName)
        {
            return new DefaultMetricsContext(contextName);
        }
    }
}
