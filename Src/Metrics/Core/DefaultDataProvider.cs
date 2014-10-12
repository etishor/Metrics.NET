using System;
using System.Collections.Generic;
using System.Linq;
using Metrics.MetricData;

namespace Metrics.Core
{
    public class DefaultDataProvider : MetricsDataProvider
    {
        private readonly string context;
        private readonly IEnumerable<EnvironmentEntry> environment;
        private readonly RegistryDataProvider registryDataProvider;
        private readonly Func<IEnumerable<MetricsDataProvider>> childProviders;

        public DefaultDataProvider(string context, RegistryDataProvider registryDataProvider, Func<IEnumerable<MetricsDataProvider>> childProviders)
            : this(context, Enumerable.Empty<EnvironmentEntry>(), registryDataProvider, childProviders)
        {
        }
        public DefaultDataProvider(string context, IEnumerable<EnvironmentEntry> environment,
            RegistryDataProvider registryDataProvider, Func<IEnumerable<MetricsDataProvider>> childProviders)
        {
            this.context = context;
            this.environment = environment;
            this.registryDataProvider = registryDataProvider;
            this.childProviders = childProviders;
        }

        public MetricsData CurrentMetricsData
        {
            get
            {
                return new MetricsData(this.context,
                    this.environment,
                    this.registryDataProvider.Gauges,
                    this.registryDataProvider.Counters,
                    this.registryDataProvider.Meters,
                    this.registryDataProvider.Histograms,
                    this.registryDataProvider.Timers,
                    this.childProviders().Select(p => p.CurrentMetricsData));
            }
        }
    }
}
