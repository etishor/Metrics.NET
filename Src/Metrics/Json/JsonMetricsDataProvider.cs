using System;
using System.Collections.Generic;
using System.Linq;
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
                return ToMetricsData(data);
            }
        }

        private static MetricsData ToMetricsData(JsonMetricsContext data)
        {
            return new MetricsData(data.Context,
                    data.Gauges.Select(g => g.ToValueSource()),
                    data.Counters.Select(c => c.ToValueSource()),
                    data.Meters.Select(m => m.ToValueSource()),
                    data.Histograms.Select(h => h.ToValueSource()),
                    data.Timers.Select(t => t.ToValueSource()),
                    data.ChildContexts.Select(c => ToMetricsData(c)));
        }
    }
}
