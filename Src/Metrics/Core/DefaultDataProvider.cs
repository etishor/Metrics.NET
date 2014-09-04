using System.Collections.Generic;
using System.Linq;

namespace Metrics.Core
{
    public class DefaultDataProvider : MetricsDataProvider
    {
        private readonly string context;
        private readonly RegistryDataProvider registryDataProvider;
        private readonly IEnumerable<MetricsDataProvider> childProviders;

        public DefaultDataProvider(string context, RegistryDataProvider registryDataProvider, IEnumerable<MetricsDataProvider> childProviders)
        {
            this.context = context;
            this.registryDataProvider = registryDataProvider;
            this.childProviders = childProviders;
        }

        public MetricsData CurrentMetricsData
        {
            get
            {
                return new MetricsData(this.context,
                    this.registryDataProvider.Gauges,
                    this.registryDataProvider.Counters,
                    this.registryDataProvider.Meters,
                    this.registryDataProvider.Histograms,
                    this.registryDataProvider.Timers,
                    this.childProviders.Select(p => p.CurrentMetricsData));
            }
        }
    }
}
