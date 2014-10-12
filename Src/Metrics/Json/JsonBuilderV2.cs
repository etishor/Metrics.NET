using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Metrics.MetricData;
using Metrics.Utils;

namespace Metrics.Json
{
    public sealed class JsonBuilderV2
    {
        public const int Version = 2;
        public const string MetricsMimeType = "application/vnd.metrics.net.v2.metrics+json";

#if !DEBUG
        private const bool DefaultIndented = false;
#else
        private const bool DefaultIndented = true;
#endif

        private readonly List<JsonProperty> root = new List<JsonProperty>();

        public static string BuildJson(MetricsData data) { return BuildJson(data, Clock.Default, indented: DefaultIndented); }

        public static string BuildJson(MetricsData data, Clock clock, bool indented = DefaultIndented)
        {
            return new JsonBuilderV2()
                .AddVersion(Version)
                .AddTimestamp(clock)
                .AddEnvironment()
                .AddData(data)
                .GetJson(indented);
        }

        private JsonBuilderV2 AddEnvironment()
        {
            var environment = AppEnvironment.Current
                .Select(e => new JsonProperty(e.Name, e.Value));

            root.Add(new JsonProperty("Environment", new ObjectJsonValue(new JsonObject(environment))));

            return this;
        }

        private JsonBuilderV2 AddVersion(int version)
        {
            root.Add(new JsonProperty("Version", version.ToString(CultureInfo.InvariantCulture)));
            return this;
        }

        private JsonBuilderV2 AddTimestamp(Clock clock)
        {
            root.Add(new JsonProperty("Timestamp", clock.UTCDateTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffK", CultureInfo.InvariantCulture)));
            return this;
        }

        private JsonBuilderV2 AddData(MetricsData data)
        {
            this.AddContext(data.Context)
                .Add(data.Gauges)
                .Add(data.Counters)
                .Add(data.Meters)
                .Add(data.Histograms)
                .Add(data.Timers)
                .Add(data.ChildMetrics);

            return this;
        }

        private JsonBuilderV2 AddContext(string context)
        {
            if (!string.IsNullOrEmpty(context))
            {
                root.Add(new JsonProperty("Context", context));
            }
            return this;
        }

        private JsonObject GetObject()
        {
            return new JsonObject(this.root);
        }

        private JsonBuilderV2 Add(IEnumerable<MetricsData> childData)
        {
            if (childData.Any())
            {
                var childObjects = childData.Select(d => new JsonBuilderV2().AddData(d).GetObject());
                root.Add(new JsonProperty("ChildContexts", childObjects));
            }
            return this;
        }

        private string GetJson(bool indented = DefaultIndented)
        {
            return new JsonObject(root).AsJson(indented);
        }

        private JsonBuilderV2 Add(IEnumerable<GaugeValueSource> gauges)
        {
            if (gauges.Any())
            {
                root.Add(new JsonProperty("Gauges", gauges.Select(g => new JsonObject(ToJsonProperties(g)))));
            }
            return this;
        }

        private static IEnumerable<JsonProperty> ToJsonProperties(GaugeValueSource gauge)
        {
            yield return new JsonProperty("Name", gauge.Name);
            yield return new JsonProperty("Value", gauge.Value);
            yield return new JsonProperty("Unit", gauge.Unit.Name);
        }

        private JsonBuilderV2 Add(IEnumerable<CounterValueSource> counters)
        {
            if (counters.Any())
            {
                root.Add(new JsonProperty("Counters", counters.Select(c => new JsonObject(ToJsonProperties(c)))));
            }
            return this;
        }

        private static IEnumerable<JsonProperty> ToJsonProperties(CounterValueSource counter)
        {
            yield return new JsonProperty("Name", counter.Name);
            yield return new JsonProperty("Value", counter.Value.Count);
            yield return new JsonProperty("Unit", counter.Unit.Name);
        }

        private JsonBuilderV2 Add(IEnumerable<MeterValueSource> meters)
        {
            if (meters.Any())
            {
                root.Add(new JsonProperty("Meters", meters.Select(m => new JsonObject(ToJsonProperties(m)))));
            }
            return this;
        }

        private static IEnumerable<JsonProperty> ToJsonProperties(MeterValueSource meter)
        {
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

        private JsonBuilderV2 Add(IEnumerable<HistogramValueSource> histograms)
        {
            if (histograms.Any())
            {
                root.Add(new JsonProperty("Histograms", histograms.Select(h => new JsonObject(ToJsonProperties(h)))));
            }
            return this;
        }

        private static IEnumerable<JsonProperty> ToJsonProperties(HistogramValueSource histogram)
        {
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
            yield return new JsonProperty("LastUserValue", value.LastUserValue);
            yield return new JsonProperty("Min", value.Min);
            yield return new JsonProperty("MinUserValue", value.MinUserValue);
            yield return new JsonProperty("Mean", value.Mean);
            yield return new JsonProperty("MaxUserValue", value.MaxUserValue);
            yield return new JsonProperty("StdDev", value.StdDev);
            yield return new JsonProperty("Median", value.Median);
            yield return new JsonProperty("Percentile75", value.Percentile75);
            yield return new JsonProperty("Percentile95", value.Percentile95);
            yield return new JsonProperty("Percentile98", value.Percentile98);
            yield return new JsonProperty("Percentile99", value.Percentile99);
            yield return new JsonProperty("Percentile999", value.Percentile999);
            yield return new JsonProperty("SampleSize", value.SampleSize);
        }

        private JsonBuilderV2 Add(IEnumerable<TimerValueSource> timers)
        {
            if (timers.Any())
            {
                root.Add(new JsonProperty("Timers", timers.Select(t => new JsonObject(ToJsonProperties(t)))));
            }
            return this;
        }

        private static IEnumerable<JsonProperty> ToJsonProperties(TimerValueSource timer)
        {
            yield return new JsonProperty("Name", timer.Name);
            yield return new JsonProperty("Rate", ToJsonProperties(timer.Value.Rate.Scale(timer.RateUnit)));
            yield return new JsonProperty("Histogram", ToJsonProperties(timer.Value.Histogram.Scale(timer.DurationUnit)));
            yield return new JsonProperty("Unit", timer.Unit.Name);
            yield return new JsonProperty("RateUnit", timer.RateUnit.Unit());
            yield return new JsonProperty("DurationUnit", timer.DurationUnit.Unit());
        }
    }
}
