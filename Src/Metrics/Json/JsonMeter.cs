
using System.Collections.Generic;
using System.Linq;
using Metrics.MetricData;
using Metrics.Utils;
namespace Metrics.Json
{
    public class JsonMeter
    {
        public class SetItem
        {
            public string Item { get; set; }
            public long Count { get; set; }
            public double MeanRate { get; set; }
            public double OneMinuteRate { get; set; }
            public double FiveMinuteRate { get; set; }
            public double FifteenMinuteRate { get; set; }
            public double Percent { get; set; }
        }

        public string Name { get; set; }
        public long Count { get; set; }
        public double MeanRate { get; set; }
        public double OneMinuteRate { get; set; }
        public double FiveMinuteRate { get; set; }
        public double FifteenMinuteRate { get; set; }
        public string Unit { get; set; }
        public string RateUnit { get; set; }
        public SetItem[] Items { get; set; }

        public static JsonMeter FromMeter(MeterValueSource meter)
        {
            return new JsonMeter
            {
                Name = meter.Name,
                Count = meter.Value.Count,
                MeanRate = meter.Value.MeanRate,
                OneMinuteRate = meter.Value.OneMinuteRate,
                FiveMinuteRate = meter.Value.FiveMinuteRate,
                FifteenMinuteRate = meter.Value.FifteenMinuteRate,
                Unit = meter.Unit.Name,
                RateUnit = meter.RateUnit.Unit(),
                Items = meter.Value.Items.Select(i => new SetItem
                {
                    Item = i.Item,
                    Count = i.Value.Count,
                    MeanRate = i.Value.MeanRate,
                    OneMinuteRate = i.Value.OneMinuteRate,
                    FiveMinuteRate = i.Value.FiveMinuteRate,
                    FifteenMinuteRate = i.Value.FifteenMinuteRate,
                    Percent = i.Percent
                }).ToArray()
            };
        }

        public JsonObject ToJsonObject()
        {
            return new JsonObject(ToJsonProperties());
        }

        public IEnumerable<JsonProperty> ToJsonProperties()
        {
            yield return new JsonProperty("Name", this.Name);
            yield return new JsonProperty("Count", this.Count);
            yield return new JsonProperty("MeanRate", this.MeanRate);
            yield return new JsonProperty("OneMinuteRate", this.OneMinuteRate);
            yield return new JsonProperty("FiveMinuteRate", this.FiveMinuteRate);
            yield return new JsonProperty("FifteenMinuteRate", this.FifteenMinuteRate);
            yield return new JsonProperty("Unit", this.Unit);
            yield return new JsonProperty("RateUnit", this.RateUnit);

            if (this.Items.Length > 0)
            {
                yield return new JsonProperty("Items", this.Items.Select(i => new JsonObject(ToJsonProperties(i))));
            }
        }

        private static IEnumerable<JsonProperty> ToJsonProperties(SetItem item)
        {
            yield return new JsonProperty("Item", item.Item);
            yield return new JsonProperty("Count", item.Count);
            yield return new JsonProperty("MeanRate", item.MeanRate);
            yield return new JsonProperty("OneMinuteRate", item.OneMinuteRate);
            yield return new JsonProperty("FiveMinuteRate", item.FiveMinuteRate);
            yield return new JsonProperty("FifteenMinuteRate", item.FifteenMinuteRate);
            yield return new JsonProperty("Percent", item.Percent);
        }
    }
}
