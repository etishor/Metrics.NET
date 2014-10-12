
using System.Collections.Generic;
using Metrics.MetricData;
namespace Metrics.Json
{
    public class JsonGauge
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }

        public static JsonGauge FromGauge(GaugeValueSource gauge)
        {
            return new JsonGauge
            {
                Name = gauge.Name,
                Value = gauge.Value,
                Unit = gauge.Unit.Name
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
        }
    }
}
