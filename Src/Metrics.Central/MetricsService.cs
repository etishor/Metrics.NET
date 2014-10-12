using Metrics.Json;
using Newtonsoft.Json;
using Topshelf;

namespace Metrics.Central
{
    public class MetricsService : ServiceControl
    {
        public bool Start(HostControl hostControl)
        {
            Metric.Config
                .WithJsonDeserialzier(s => JsonConvert.DeserializeObject<JsonMetricsContext>(s))
                .WithSystemCounters()
                .WithCLRGlobalCounters();
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            return true;
        }
    }
}
