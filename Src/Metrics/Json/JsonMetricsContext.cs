using System.Collections.Generic;
using System.Linq;
using Metrics.MetricData;
using Metrics.Utils;

namespace Metrics.Json
{
    public class JsonMetricsContext
    {
        public string Version { get; set; }
        public string Timestamp { get; set; }
        public Dictionary<string, string> Environment { get; set; }

        public string Context { get; set; }
        public JsonGauge[] Gauges { get; set; }
        public JsonCounter[] Counters { get; set; }
        public JsonMeter[] Meters { get; set; }
        public JsonHistogram[] Histograms { get; set; }
        public JsonTimer[] Timers { get; set; }

        public JsonMetricsContext[] ChildContexts { get; set; }

        public static JsonMetricsContext FromContext(MetricsData contextData)
        {
            return FromContext(contextData, null, null, null);
        }

        public static JsonMetricsContext FromContext(MetricsData contextData, string version, string timestamp, AppEnvironment.Entry[] environment)
        {
            return new JsonMetricsContext
            {
                Version = version,
                Timestamp = timestamp,
                Environment = ToEnvironment(environment ?? new AppEnvironment.Entry[0]),

                Context = contextData.Context,
                Gauges = contextData.Gauges.Select(g => JsonGauge.FromGauge(g)).ToArray(),
                Counters = contextData.Counters.Select(c => JsonCounter.FromCounter(c)).ToArray(),
                Meters = contextData.Meters.Select(m => JsonMeter.FromMeter(m)).ToArray(),
                Histograms = contextData.Histograms.Select(h => JsonHistogram.FromHistogram(h)).ToArray(),
                Timers = contextData.Timers.Select(t => JsonTimer.FromTimer(t)).ToArray(),
                ChildContexts = contextData.ChildMetrics.Select(c => JsonMetricsContext.FromContext(c)).ToArray()
            };
        }

        private static Dictionary<string, string> ToEnvironment(AppEnvironment.Entry[] environment)
        {
            return environment.ToDictionary(e => e.Name, e => e.Value);
        }

        public JsonObject ToJsonObject()
        {
            return new JsonObject(ToJsonProperties());
        }

        private IEnumerable<JsonProperty> ToJsonProperties()
        {
            if (!string.IsNullOrEmpty(this.Version))
            {
                yield return new JsonProperty("Version", this.Version);
            }

            if (!string.IsNullOrEmpty(this.Timestamp))
            {
                yield return new JsonProperty("Timestamp", this.Timestamp);
            }

            if (this.Environment.Count > 0)
            {
                yield return new JsonProperty("Environment", this.Environment.Select(e => new JsonProperty(e.Key, e.Value)));
            }

            if (!string.IsNullOrEmpty(this.Context))
            {
                yield return new JsonProperty("Context", this.Context);
            }

            if (this.Gauges.Length > 0)
            {
                yield return new JsonProperty("Gauges", this.Gauges.Select(g => g.ToJsonObject()));
            }

            if (this.Counters.Length > 0)
            {
                yield return new JsonProperty("Counters", this.Counters.Select(c => c.ToJsonObject()));
            }

            if (this.Meters.Length > 0)
            {
                yield return new JsonProperty("Meters", this.Meters.Select(m => m.ToJsonObject()));
            }

            if (this.Histograms.Length > 0)
            {
                yield return new JsonProperty("Histograms", this.Histograms.Select(h => h.ToJsonObject()));
            }

            if (this.Timers.Length > 0)
            {
                yield return new JsonProperty("Timers", this.Timers.Select(t => t.ToJsonTimer()));
            }

            if (this.ChildContexts.Length > 0)
            {
                yield return new JsonProperty("ChildContexts", this.ChildContexts.Select(c => c.ToJsonObject()));
            }
        }
    }
}

