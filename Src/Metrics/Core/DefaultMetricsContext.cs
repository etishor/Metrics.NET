
using Metrics.Core;
namespace Metrics
{
    public sealed class DefaultMetricsContext : BaseMetricsContext
    {
        public DefaultMetricsContext()
            : this(string.Empty) { }

        public DefaultMetricsContext(string context)
            : this(context, new LocalRegistry(context))
        { }

        public DefaultMetricsContext(string context, MetricsRegistry registry)
            : base(context, registry)
        {
        }

        protected override MetricsContext CreateChildContextInstance(string contextName)
        {
            return new DefaultMetricsContext(contextName);
        }
    }
}
