using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Metrics.Core;
using Metrics.Utils;

namespace Metrics.Json
{
    public sealed class JsonBuilder
    {
        private readonly List<JsonProperty> root = new List<JsonProperty>();

        private readonly Dictionary<string, List<JsonProperty>> contextRoot = new Dictionary<string, List<JsonProperty>>();

        public class ContextResult
        {
            public string Context { get; set; }
            public IEnumerable<GaugeValueSource> Gauges { get; set; }
            public IEnumerable<CounterValueSource> Counters { get; set; }
            public IEnumerable<MeterValueSource> Meters { get; set; }
            public IEnumerable<HistogramValueSource> Histograms { get; set; }
            public IEnumerable<TimerValueSource> Timers { get; set; }
        }

        public string BuildJson(MetricsRegistry registry, MetricsFilter filter, bool indented = true)
        {
            Dictionary<string, ContextResult> contexts = new Dictionary<string, ContextResult>();

            registry.Gauges
                .Where(g => filter.IsMatch(g))
                .GroupBy(g => g.Context)
                .Select(g => new
                {
                    Context = g.Key,
                    Gauges = g.AsEnumerable()
                });

            throw new NotImplementedException();

        }

        public string GetJson(bool indented = true)
        {
            return new JsonObject(root).AsJson(indented);
        }

        public JsonBuilder AddTimestamp(Clock clock)
        {
            root.Add(new JsonProperty("Timestamp", clock.LocalDateTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffK", CultureInfo.InvariantCulture)));
            return this;
        }

        public JsonBuilder Add(IEnumerable<GaugeValueSource> gauges)
        {
            root.Add(new JsonProperty("Gauges", gauges.Select(g => new JsonObject(ToJsonProperties(g)))));
            return this;
        }

        private static IEnumerable<JsonProperty> ToJsonProperties(GaugeValueSource gauge)
        {
            yield return new JsonProperty("Context", gauge.Context);
            yield return new JsonProperty("Name", gauge.Name);
            yield return new JsonProperty("Value", gauge.Value);
            yield return new JsonProperty("Unit", gauge.Unit.Name);
        }

        public JsonBuilder Add(IEnumerable<CounterValueSource> counters)
        {
            root.Add(new JsonProperty("Counters", counters.Select(c => new JsonObject(ToJsonProperties(c)))));
            return this;
        }

        private static IEnumerable<JsonProperty> ToJsonProperties(CounterValueSource counter)
        {
            yield return new JsonProperty("Context", counter.Context);
            yield return new JsonProperty("Name", counter.Name);
            yield return new JsonProperty("Value", counter.Value);
            yield return new JsonProperty("Unit", counter.Unit.Name);
        }

        public JsonBuilder Add(IEnumerable<MeterValueSource> meters)
        {
            root.Add(new JsonProperty("Meters", meters.Select(m => new JsonObject(ToJsonProperties(m)))));
            return this;
        }

        private static IEnumerable<JsonProperty> ToJsonProperties(MeterValueSource meter)
        {
            yield return new JsonProperty("Context", meter.Context);
            yield return new JsonProperty("Name", meter.Name);
            foreach (var property in ToJsonProperties(meter.Value))
            {
                yield return property;
            }
            yield return new JsonProperty("Unit", meter.Unit.Name);
            yield return new JsonProperty("RateUnit", meter.RateUnit.Unit());
        }

        private static IEnumerable<JsonProperty> ToJsonProperties(MeterValue value)
        {
            yield return new JsonProperty("Count", value.Count);
            yield return new JsonProperty("MeanRate", value.MeanRate);
            yield return new JsonProperty("OneMinuteRate", value.OneMinuteRate);
            yield return new JsonProperty("FiveMinuteRate", value.FiveMinuteRate);
            yield return new JsonProperty("FifteenMinuteRate", value.FifteenMinuteRate);
        }

        public JsonBuilder Add(IEnumerable<HistogramValueSource> histograms)
        {
            root.Add(new JsonProperty("Histograms", histograms.Select(h => new JsonObject(ToJsonProperties(h)))));
            return this;
        }

        private static IEnumerable<JsonProperty> ToJsonProperties(HistogramValueSource histogram)
        {
            yield return new JsonProperty("Context", histogram.Context);
            yield return new JsonProperty("Name", histogram.Name);
            foreach (var property in ToJsonProperties(histogram.Value))
            {
                yield return property;
            }
            yield return new JsonProperty("Unit", histogram.Unit.Name);
        }

        private static IEnumerable<JsonProperty> ToJsonProperties(HistogramValue value)
        {
            yield return new JsonProperty("Count", value.Count);
            yield return new JsonProperty("LastValue", value.LastValue);
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

        public JsonBuilder Add(IEnumerable<TimerValueSource> timers)
        {
            root.Add(new JsonProperty("Timers", timers.Select(t => new JsonObject(ToJsonProperties(t)))));
            return this;
        }

        private static IEnumerable<JsonProperty> ToJsonProperties(TimerValueSource timer)
        {
            yield return new JsonProperty("Context", timer.Context);
            yield return new JsonProperty("Name", timer.Name);
            yield return new JsonProperty("Rate", ToJsonProperties(timer.Value.Rate.Scale(timer.RateUnit)));
            yield return new JsonProperty("Histogram", ToJsonProperties(timer.Value.Histogram.Scale(timer.DurationUnit)));
            yield return new JsonProperty("Unit", timer.Unit.Name);
            yield return new JsonProperty("RateUnit", timer.RateUnit.Unit());
            yield return new JsonProperty("DurationUnit", timer.DurationUnit.Unit());
        }
    }
}
