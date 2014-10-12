
using System.Collections.Generic;
using System.Linq;
using Metrics.MetricData;
namespace Metrics.Json
{
    public class JsonCounter
    {
        public class SetItem
        {
            public string Item { get; set; }
            public long Count { get; set; }
            public double Percent { get; set; }
        }

        public string Name { get; set; }
        public long Count { get; set; }
        public string Unit { get; set; }
        public SetItem[] Items { get; set; }

        public static JsonCounter FromCounter(CounterValueSource counter)
        {
            return new JsonCounter
            {
                Name = counter.Name,
                Count = counter.Value.Count,
                Unit = counter.Unit.Name,
                Items = counter.Value.Items.Select(i => new SetItem { Item = i.Item, Count = i.Count, Percent = i.Percent }).ToArray()
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
            yield return new JsonProperty("Unit", this.Unit);

            if (this.Items.Length > 0)
            {
                yield return new JsonProperty("Items", this.Items.Select(i => new JsonObject(ToJsonProperties(i))));
            }
        }

        private static IEnumerable<JsonProperty> ToJsonProperties(SetItem item)
        {
            yield return new JsonProperty("Item", item.Item);
            yield return new JsonProperty("Count", item.Count);
            yield return new JsonProperty("Percent", item.Percent);
        }
    }
}
