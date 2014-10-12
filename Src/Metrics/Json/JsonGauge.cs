
using System.Collections.Generic;
using Metrics.MetricData;

namespace Metrics.Json
{
    public class JsonGauge : JsonMetric
    {
        public double Value { get; set; }

        public static JsonGauge FromGauge(GaugeValueSource gauge)
        {
            return new JsonGauge
            {
                Name = gauge.Name,
                Value = gauge.Value,
                Unit = gauge.Unit.Name,
                Tags = gauge.Tags
            };
        }

        public JsonObject ToJsonObject()
        {
            return new JsonObject(ToJsonProperties());
        }

        public IEnumerable<JsonProperty> ToJsonProperties()
        {
            yield return new JsonProperty("Name", this.Name);
            yield return new JsonProperty("Value", this.Value);
            yield return new JsonProperty("Unit", this.Unit);

            if (this.Tags.Length > 0)
            {
                yield return new JsonProperty("Tags", this.Tags);
            }
        }

        public GaugeValueSource ToValueSource()
        {
            return new GaugeValueSource(this.Name, ConstantValue.Provider(this.Value), this.Unit, this.Tags);
        }
    }
}
