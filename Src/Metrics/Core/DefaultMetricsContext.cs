
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

        public override MetricsContext Context(string contextName)
        {
            if (string.IsNullOrEmpty(contextName))
            {
                return this;
            }

            return this.Context(contextName, c => new DefaultMetricsContext(contextName));
        }
    }
}
