using System;
using Metrics.MetricData;

namespace Metrics.Json
{
    public sealed class JsonMetricsDataProvider : MetricsDataProvider
    {
        private readonly Func<JsonMetricsContext> jsonDataProvider;
        public JsonMetricsDataProvider(Func<JsonMetricsContext> jsonDataProvider)
        {
            this.jsonDataProvider = jsonDataProvider;
        }

        public MetricsData CurrentMetricsData
        {
            get
            {
                var data = jsonDataProvider();

                if (data == null)
                {
                    return MetricsData.Empty;
                }

                return data.ToMetricsData();
            }
        }
    }
}
