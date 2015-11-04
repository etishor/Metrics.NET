using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Metrics.Reporters;
using Metrics.Utils;

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
        private readonly HttpClient httpClient;
        
        private List<InfluxRecord> data;

        public InfluxdbReport(Uri influxdb, string username, string password)
        {
            this.influxdb = influxdb;

            var byteArray = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", username, password));

            httpClient = new HttpClient()
            {
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray))
                }
            };
        }

        private class InfluxRecord
        {
            public InfluxRecord(string name, long timestamp, IEnumerable<string> columns, IEnumerable<object> data)
            {
                // see: https://influxdb.com/docs/v0.9/write_protocols/write_syntax.html

                var record = new StringBuilder();
                record.Append(Normalize(name)).Append(" ");

                var fieldKeypairs = new List<string>();

                foreach (var pair in columns.Zip(data, (col, dat) => new Tuple<string, object>(col, dat)))
                {
                    fieldKeypairs.Add(string.Format("{0}={1}", Normalize(pair.Item1), pair.Item2));
                }

                record.Append(string.Join(",", fieldKeypairs));

                record.Append(" ").Append(string.Format("{0:F0}", timestamp * 1e9));

                LineProtocol = record.ToString();
            }

            private static string Normalize(string v)
            {
                return v.Replace(" ", "\\ ");
            }

            public string LineProtocol { get; private set; }
        }

        private void Pack(string name, IEnumerable<string> columns, object value)
        {
            Pack(name, columns, Enumerable.Repeat(value, 1));
        }

        private void Pack(string name, IEnumerable<string> columns, IEnumerable<object> values)
        {
            this.data.Add(new InfluxRecord(name, CurrentContextTimestamp.ToUnixTime(), columns, values));
        }

        protected override void StartReport(string contextName)
        {
            this.data = new List<InfluxRecord>();
            base.StartReport(contextName);
        }

        protected override void EndReport(string contextName)
        {
            using (Metric.Context("Metrics.NET").Timer("influxdb.report", Unit.Calls).NewContext())
            {
                base.EndReport(contextName);

                var content = string.Join("\n", data.Select(d => d.LineProtocol));

                var task = httpClient.PostAsync(influxdb, new StringContent(content));

                data = null;

                task.ContinueWith(m =>
                {
                    if (m.Result.IsSuccessStatusCode)
                    {
                        Metric.Context("Metrics.NET").Counter("influxdb.report.success", Unit.Events).Increment();
                    }
                    else
                    {
                        Metric.Context("Metrics.NET").Counter("influxdb.report.fail", Unit.Events).Increment();
                    }
                });
            }
        }

        protected override void ReportGauge(string name, double value, Unit unit, MetricTags tags)
        {
            if (!double.IsNaN(value) && !double.IsInfinity(value))
            {
                Pack(name, GaugeColumns, value);
            }
        }

        protected override void ReportCounter(string name, MetricData.CounterValue value, Unit unit, MetricTags tags)
        {
            var itemColumns = value.Items.SelectMany(i => new[] { i.Item + " - Count", i.Item + " - Percent" });
            var columns = CounterColumns.Concat(itemColumns);

            var itemValues = value.Items.SelectMany(i => new[] { i.Count, i.Percent });
            foreach (var dat in new[] {(double) value.Count}.Concat(itemValues))
            {
                Pack(name, columns, dat);
            }
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
                i.Value.Count, 
                i.Percent,
                i.Value.MeanRate, 
                i.Value.OneMinuteRate, 
                i.Value.FiveMinuteRate, 
                i.Value.FifteenMinuteRate
            });

            var data = new[] 
            {
                value.Count,
                value.MeanRate,
                value.OneMinuteRate,
                value.FiveMinuteRate,
                value.FifteenMinuteRate
            }.Concat(itemValues);

            foreach (var dat in data)
            {
                Pack(name, columns, dat);
            }
        }

        protected override void ReportHistogram(string name, MetricData.HistogramValue value, Unit unit, MetricTags tags)
        {
            Pack(name, HistogramColumns, new object[]{
                value.Count,
                value.LastValue,
                value.LastUserValue ?? "\"\"",
                value.Min,
                value.MinUserValue ?? "\"\"",
                value.Mean,
                value.Max,
                value.MaxUserValue ?? "\"\"",
                value.StdDev,
                value.Median,
                value.Percentile75,
                value.Percentile95,
                value.Percentile98,
                value.Percentile99,
                value.Percentile999,
                value.SampleSize
            });
        }

        protected override void ReportTimer(string name, MetricData.TimerValue value, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags)
        {
            Pack(name, TimerColumns, new object[]{

                value.Rate.Count,
                value.ActiveSessions,
                value.Rate.MeanRate,
                value.Rate.OneMinuteRate,
                value.Rate.FiveMinuteRate,
                value.Rate.FifteenMinuteRate,
                value.Histogram.LastValue,
                value.Histogram.LastUserValue ?? "\"\"",
                value.Histogram.Min,
                value.Histogram.MinUserValue ?? "\"\"",
                value.Histogram.Mean,
                value.Histogram.Max,
                value.Histogram.MaxUserValue ?? "\"\"",
                value.Histogram.StdDev,
                value.Histogram.Median,
                value.Histogram.Percentile75,
                value.Histogram.Percentile95,
                value.Histogram.Percentile98,
                value.Histogram.Percentile99,
                value.Histogram.Percentile999,
                value.Histogram.SampleSize
            });
        }

        protected override void ReportHealth(HealthStatus status)
        {
        }
    }
}
