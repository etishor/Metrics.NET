
using Metrics;
namespace Nancy.Metrics
{
    public class MetricsModule : NancyModule
    {
        public MetricsModule()
            : base(NancyMetrics.MetricsModulePath)
        {
            if (string.IsNullOrEmpty(NancyMetrics.MetricsModulePath))
            {
                return;
            }

            NancyMetrics.MetricsModuleConfig(this);

            Get["/"] = _ => Response.AsText(Metric.GetAsHumanReadable());
            Get["/json"] = _ => Response.AsJson(Metric.GetForSerialization());

        }
    }
}
