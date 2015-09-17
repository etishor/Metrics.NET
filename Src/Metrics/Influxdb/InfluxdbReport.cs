using Metrics.Json;
using Metrics.Reporters;
using Metrics.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Metrics.Influxdb
{
    public class InfluxdbReport : BaseReport
    {
        private static readonly string[] GaugeColumns = new[] { "Value" };
        private static readonly string[] CounterColumns = new[] { "Count" };
        private static readonly string[] MeterColumns = new[] { "Total Count", "Mean Rate", "1 Min Rate", "5 Min Rate", "15 Min Rate", };
        private static readonly string[] HistogramColumns = new[]
        {
            "Total Count", "Last", "Last User Value", "Min", "Min User Value", "Mean", "Max", "Max User Value",
            "StdDev", "Median", "Percentile 75%", "Percentile 95%", "Percentile 98%", "Percentile 99%", "Percentile 99.9%" , "Sample Size" };

        private static readonly string[] TimerColumns = new[]
        {
            "Total Count", "Active Sessions",
            "Mean Rate", "1 Min Rate", "5 Min Rate", "15 Min Rate",
            "Last", "Last User Value", "Min", "Min User Value", "Mean", "Max", "Max User Value",
            "StdDev", "Median", "Percentile 75%", "Percentile 95%", "Percentile 98%", "Percentile 99%", "Percentile 99.9%" , "Sample Size"
        };

        private readonly Uri influxdb;

        private List<InfluxRecord> data;

        public InfluxdbReport(Uri influxdb)
        {
            this.influxdb = influxdb;
        }

        private class InfluxRecord
        {
            public InfluxRecord(string name, long timestamp, IEnumerable<string> columns, IEnumerable<JsonValue> data)
            {
                var points = new[] { new LongJsonValue(timestamp) }.Concat(data);

                this.Json = new JsonObject(new[] {
                    new JsonProperty("name",name),
                    new JsonProperty("columns", new []{"time"}.Concat(columns)),
                    new JsonProperty("points", new JsonValueArray( new [] { new JsonValueArray( points)}))
                });
            }

            public JsonObject Json { get; }
        }

        private void Pack(string name, IEnumerable<string> columns, JsonValue value)
        {
            Pack(name, columns, Enumerable.Repeat(value, 1));
        }

        private void Pack(string name, IEnumerable<string> columns, IEnumerable<JsonValue> values)
        {
            this.data.Add(new InfluxRecord(name, CurrentContextTimestamp.ToUnixTime(), columns, values));
        }

        private JsonValue Value(long value)
        {
            return new LongJsonValue(value);
        }

        private JsonValue Value(double value)
        {
            return new DoubleJsonValue(value);
        }

        private JsonValue Value(string value)
        {
            return new StringJsonValue(value);
        }

        protected override void StartReport(string contextName)
        {
            this.data = new List<InfluxRecord>();
            base.StartReport(contextName);
        }

        protected override void EndReport(string contextName)
        {
            base.EndReport(contextName);

            using (var client = new WebClient())
            {
                var json = new CollectionJsonValue(data.Select(d => d.Json)).AsJson();
                client.UploadString(this.influxdb, json);
            }
            this.data = null;
        }

        protected override void ReportGauge(string name, double value, Unit unit, MetricTags tags)
        {
            if (!double.IsNaN(value) && !double.IsInfinity(value))
            {
                Pack(name, GaugeColumns, Value(value));
            }
        }

        protected override void ReportCounter(string name, MetricData.CounterValue value, Unit unit, MetricTags tags)
        {
            var itemColumns = value.Items.SelectMany(i => new[] { i.Item + " - Count", i.Item + " - Percent" });
            var columns = CounterColumns.Concat(itemColumns);

            var itemValues = value.Items.SelectMany(i => new[] { Value(i.Count), Value(i.Percent) });
            var data = new[] { Value(value.Count) }.Concat(itemValues);

            Pack(name, columns, data);
        }

        protected override void ReportMeter(string name, MetricData.MeterValue value, Unit unit, TimeUnit rateUnit, MetricTags tags)
        {
            var itemColumns = value.Items.SelectMany(i => new[]
            {
                i.Item + " - Count",
                i.Item + " - Percent",
                i.Item + " - Mean Rate",
                i.Item + " - 1 Min Rate",
                i.Item + " - 5 Min Rate",
                i.Item + " - 15 Min Rate"
            });
            var columns = MeterColumns.Concat(itemColumns);

            var itemValues = value.Items.SelectMany(i => new[]
            {
                Value(i.Value.Count),
                Value(i.Percent),
                Value(i.Value.MeanRate),
                Value(i.Value.OneMinuteRate),
                Value(i.Value.FiveMinuteRate),
                Value(i.Value.FifteenMinuteRate)
            });

            var data = new[]
            {
                Value(value.Count),
                Value (value.MeanRate),
                Value (value.OneMinuteRate),
                Value (value.FiveMinuteRate),
                Value (value.FifteenMinuteRate)
            }.Concat(itemValues);

            Pack(name, columns, data);
        }

        protected override void ReportHistogram(string name, MetricData.HistogramValue value, Unit unit, MetricTags tags)
        {
            Pack(name, HistogramColumns, new[]{
                Value(value.Count),
                Value(value.LastValue),
                Value(value.LastUserValue),
                Value(value.Min),
                Value(value.MinUserValue),
                Value(value.Mean),
                Value(value.Max),
                Value(value.MaxUserValue),
                Value(value.StdDev),
                Value(value.Median),
                Value(value.Percentile75),
                Value(value.Percentile95),
                Value(value.Percentile98),
                Value(value.Percentile99),
                Value(value.Percentile999),
                Value(value.SampleSize)
            });
        }

        protected override void ReportTimer(string name, MetricData.TimerValue value, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags)
        {
            Pack(name, TimerColumns, new[]{

                Value(value.Rate.Count),
                Value(value.ActiveSessions),
                Value(value.Rate.MeanRate),
                Value(value.Rate.OneMinuteRate),
                Value(value.Rate.FiveMinuteRate),
                Value(value.Rate.FifteenMinuteRate),
                Value(value.Histogram.LastValue),
                Value(value.Histogram.LastUserValue),
                Value(value.Histogram.Min),
                Value(value.Histogram.MinUserValue),
                Value(value.Histogram.Mean),
                Value(value.Histogram.Max),
                Value(value.Histogram.MaxUserValue),
                Value(value.Histogram.StdDev),
                Value(value.Histogram.Median),
                Value(value.Histogram.Percentile75),
                Value(value.Histogram.Percentile95),
                Value(value.Histogram.Percentile98),
                Value(value.Histogram.Percentile99),
                Value(value.Histogram.Percentile999),
                Value(value.Histogram.SampleSize)
            });
        }

        protected override void ReportHealth(HealthStatus status)
        {
        }
    }
}
