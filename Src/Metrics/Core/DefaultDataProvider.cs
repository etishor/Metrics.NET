using System;
using System.Collections.Generic;
using System.Linq;

namespace Metrics.Core
{
    public class DefaultDataProvider : MetricsDataProvider
    {
        private readonly string context;
        private readonly Func<MetricsData> registryData;
        private readonly Func<IEnumerable<MetricsDataProvider>> childProviders;

        public DefaultDataProvider(string context, Func<MetricsData> registryData, Func<IEnumerable<MetricsDataProvider>> childProviders)
        {
            this.context = context;
            this.registryData = registryData;
            this.childProviders = childProviders;
        }

        public MetricsData CurrentMetricsData
        {
            get
            {
                var data = registryData();

                var childData = childProviders().Select(p => p.CurrentMetricsData);

                return new MetricsData(this.context, data, childData);
            }
        }
    }
}
