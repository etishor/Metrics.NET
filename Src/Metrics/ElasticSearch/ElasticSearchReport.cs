using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Metrics.Json;
using Metrics.Reporters;
using Metrics.Utils;

namespace Metrics.ElasticSearch
{
    public class ElasticSearchReport : BaseReport
    {
        private readonly Uri elasticSearchUri;
        private readonly string elasticSearchIndex;

        private class ESDocument
        {
            public string Index { get; set; }
            public string Type { get; set; }
            public JsonObject Object { get; set; }
            public string ToJsonString()
            {
                var meta = string.Format("{{ \"index\" : {{ \"_index\" : \"{0}\", \"_type\" : \"{1}\"}} }}", this.Index, this.Type);
                return meta + Environment.NewLine + this.Object.AsJson(false);
            }
        }

        private List<ESDocument> data = null;

        public ElasticSearchReport(Uri elasticSearchUri, string elasticSearchIndex)
        {
            this.elasticSearchUri = elasticSearchUri;
            this.elasticSearchIndex = elasticSearchIndex;
        }


        protected override void StartReport(string contextName)
        {
            this.data = new List<ESDocument>();
            base.StartReport(contextName);
        }

        protected override void EndReport(string contextName)
        {
            base.EndReport(contextName);
            using (var client = new WebClient())
            {
                var json = string.Join(Environment.NewLine, this.data.Select(d => d.ToJsonString()));
                client.UploadString(this.elasticSearchUri, json);
            }
        }

        private void Pack(string type, string name, Unit unit, MetricTags tags, IEnumerable<JsonProperty> properties)
        {
            this.data.Add(new ESDocument
            {
                Index = this.elasticSearchIndex,
                Type = type,
                Object = new JsonObject(new[] { 
                         new JsonProperty("Timestamp", Clock.FormatTimestamp(this.CurrentContextTimestamp)),
                         new JsonProperty("Type",type),
                         new JsonProperty("Name",name),
                         new JsonProperty("Unit", unit.ToString()),
                         new JsonProperty("Tags", tags.Tags)
                     }.Concat(properties))
            });
        }

        protected override void ReportGauge(string name, double value, Unit unit, MetricTags tags)
        {
            if (!double.IsNaN(value) && !double.IsInfinity(value))
            {
                Pack("Gauge", name, unit, tags, new[] { 
                    new JsonProperty("Value", value),
                });
            }
        }

        protected override void ReportCounter(string name, MetricData.CounterValue value, Unit unit, MetricTags tags)
        {
            var itemProperties = value.Items.SelectMany(i => new[] 
            {
                new JsonProperty(i.Item + " - Count", i.Count), 
                new JsonProperty(i.Item + " - Percent", i.Percent),
            });

            Pack("Counter", name, unit, tags, new[] { 
                new JsonProperty("Count", value.Count),
            }.Concat(itemProperties));
        }

        protected override void ReportMeter(string name, MetricData.MeterValue value, Unit unit, TimeUnit rateUnit, MetricTags tags)
        {
            var itemProperties = value.Items.SelectMany(i => new[] 
            {
                new JsonProperty(i.Item + " - Count", i.Value.Count), 
                new JsonProperty(i.Item + " - Percent", i.Percent),
                new JsonProperty(i.Item + " - Mean Rate", i.Value.MeanRate),
                new JsonProperty(i.Item + " - 1 Min Rate", i.Value.OneMinuteRate),
                new JsonProperty(i.Item + " - 5 Min Rate", i.Value.FiveMinuteRate),
                new JsonProperty(i.Item + " - 15 Min Rate", i.Value.FifteenMinuteRate)
            });

            Pack("Meter", name, unit, tags, new[] { 
                new JsonProperty("Count", value.Count),
                new JsonProperty("Mean Rate", value.MeanRate),
                new JsonProperty("1 Min Rate", value.OneMinuteRate),
                new JsonProperty("5 Min Rate", value.FiveMinuteRate),
                new JsonProperty("15 Min Rate", value.FifteenMinuteRate)
            }.Concat(itemProperties));

        }

        protected override void ReportHistogram(string name, MetricData.HistogramValue value, Unit unit, MetricTags tags)
        {

        }

        protected override void ReportTimer(string name, MetricData.TimerValue value, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags)
        {

        }

        protected override void ReportHealth(HealthStatus status)
        {
        }
    }
}
