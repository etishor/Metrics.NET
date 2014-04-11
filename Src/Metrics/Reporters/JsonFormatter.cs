using System.Collections.Generic;
using System.Linq;
using Metrics.Meta;
using Metrics.Utils;

namespace Metrics.Reporters
{
    public class JsonFormatter
    {
        private readonly List<JsonProperty> root = new List<JsonProperty>();
        private readonly List<JsonProperty> units = new List<JsonProperty>();

        public string GetJson(bool indented = true)
        {
            if (!root.Any(p => p.Name == "Units"))
            {
                root.Add(new JsonProperty("Units", units));
            }

            return new JsonObject(root).AsJson(indented);
        }

        public JsonFormatter AddObject(IEnumerable<GaugeMeta> gauges)
        {
            root.Add(new JsonProperty("Gauges", gauges.Select(g => new JsonProperty(g.Name, GetJsonValue(g.Value.Value)))));
            units.Add(new JsonProperty("Gauges", gauges.Select(g => new JsonProperty(g.Name, g.Unit.Name))));
            return this;
        }

        public JsonFormatter AddObject(IEnumerable<CounterMeta> counters)
        {
            root.Add(new JsonProperty("Counters", counters.Select(c => new JsonProperty(c.Name, c.Value))));
            units.Add(new JsonProperty("Counters", counters.Select(c => new JsonProperty(c.Name, c.Unit.Name))));
            return this;
        }

        public JsonFormatter AddObject(IEnumerable<MeterMeta> meters)
        {
            root.Add(new JsonProperty("Meters", meters.Select(m => new JsonProperty(m.Name, Meter(m.Value)))));
            units.Add(new JsonProperty("Meters", meters.Select(m => new JsonProperty(m.Name, string.Format("{0}/{1}", m.Unit.Name, m.RateUnit.Unit())))));
            return this;
        }

        public JsonFormatter AddObject(IEnumerable<HistogramMeta> histograms)
        {
            root.Add(new JsonProperty("Histograms", histograms.Select(m => new JsonProperty(m.Name, Histogram(m.Value)))));
            units.Add(new JsonProperty("Histograms", histograms.Select(m => new JsonProperty(m.Name, m.Unit.Name))));
            return this;
        }

        public JsonFormatter AddObject(IEnumerable<TimerMeta> timers)
        {
            var properties = timers.Select(t => new { Name = t.Name, Value = t.Value })
                .Select(t => new JsonProperty(t.Name, new[] { new JsonProperty("Rate", Meter(t.Value.Rate)), new JsonProperty("Histogram", Histogram(t.Value.Histogram)) }));

            root.Add(new JsonProperty("Timers", properties));

            var units = timers.Select(t => new JsonProperty(t.Name, new[]
                {
                    new JsonProperty("Rate", string.Format("{0}/{1}", t.Unit.Name, t.RateUnit.Unit())),
                    new JsonProperty("Duration", t.DurationUnit.Unit())
                }));

            this.units.Add(new JsonProperty("Timers", units));

            return this;
        }

        private IEnumerable<JsonProperty> Meter(MeterValue value)
        {
            yield return new JsonProperty("Count", value.Count);
            yield return new JsonProperty("MeanRate", value.MeanRate);
            yield return new JsonProperty("OneMinuteRate", value.OneMinuteRate);
            yield return new JsonProperty("FiveMinuteRate", value.FiveMinuteRate);
            yield return new JsonProperty("FifteenMinuteRate", value.FifteenMinuteRate);
        }

        private IEnumerable<JsonProperty> Histogram(HistogramValue value)
        {
            yield return new JsonProperty("Count", value.Count);
            yield return new JsonProperty("Min", value.Min);
            yield return new JsonProperty("Mean", value.Mean);
            yield return new JsonProperty("Max", value.Max);
            yield return new JsonProperty("StdDev", value.StdDev);
            yield return new JsonProperty("Median", value.Median);
            yield return new JsonProperty("Percentile75", value.Percentile75);
            yield return new JsonProperty("Percentile95", value.Percentile95);
            yield return new JsonProperty("Percentile98", value.Percentile98);
            yield return new JsonProperty("Percentile99", value.Percentile99);
            yield return new JsonProperty("Percentile999", value.Percentile999);
            yield return new JsonProperty("SampleSize", value.SampleSize);
        }

        private JsonValue GetJsonValue(string value)
        {
            long longValue;
            if (long.TryParse(value, out longValue))
            {
                return new LongJsonValue(longValue);
            }

            double doubleValue;
            if (double.TryParse(value, out doubleValue))
            {
                return new DoubleJsonValue(doubleValue);
            }

            return new StringJsonValue(value);
        }
    }
}
