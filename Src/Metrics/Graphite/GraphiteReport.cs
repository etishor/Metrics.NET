using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Metrics.MetricData;
using Metrics.Reporters;
using Metrics.Utils;

namespace Metrics.Graphite
{
    public class GraphiteReport : BaseReport, IDisposable
    {
        private static readonly Regex invalid = new Regex(@"[^a-zA-Z0-9\-%&]+", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex slash = new Regex(@"\s*/\s*", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        private readonly GraphiteSender sender;

        private DateTime lastTimestamp;

        public GraphiteReport(GraphiteSender sender)
        {
            this.sender = sender;
        }

        protected override void StartContext(string contextName, System.DateTime timestamp)
        {
            this.lastTimestamp = timestamp;
            base.StartContext(contextName, timestamp);
        }

        protected override void EndReport(string contextName, DateTime timestamp)
        {
            base.EndReport(contextName, timestamp);
            this.sender.Flush();
        }

        protected override void ReportGauge(string name, double value, Unit unit, MetricTags tags)
        {
            if (!double.IsNaN(value) && !double.IsInfinity(value))
            {
                Send(Name(name, unit), value, this.lastTimestamp);
            }
        }

        protected override void ReportCounter(string name, CounterValue value, Unit unit, MetricTags tags)
        {
            if (value.Items.Length == 0)
            {
                Send(Name(name, unit), value.Count, this.lastTimestamp);
            }
            else
            {
                Send(SubfolderName(name, unit, "Total"), value.Count, this.lastTimestamp);
            }

            foreach (var item in value.Items)
            {
                Send(SubfolderName(name, unit, item.Item), item.Count, this.lastTimestamp);
                Send(SubfolderName(name, unit, item.Item, "Percent"), item.Percent, this.lastTimestamp);
            }
        }

        protected override void ReportMeter(string name, MeterValue value, Unit unit, TimeUnit rateUnit, MetricTags tags)
        {
            Send(SubfolderName(name, unit, "Total"), value.Count, this.lastTimestamp);
            Send(SubfolderName(name, AsRate(unit, rateUnit), "Rate-Mean"), value.MeanRate, this.lastTimestamp);
            Send(SubfolderName(name, AsRate(unit, rateUnit), "Rate-1-min"), value.OneMinuteRate, this.lastTimestamp);
            Send(SubfolderName(name, AsRate(unit, rateUnit), "Rate-5-min"), value.FiveMinuteRate, this.lastTimestamp);
            Send(SubfolderName(name, AsRate(unit, rateUnit), "Rate-15-min"), value.FifteenMinuteRate, this.lastTimestamp);

            foreach (var item in value.Items)
            {
                Send(SubfolderName(name, unit, item.Item, "Count"), item.Percent, this.lastTimestamp);
                Send(SubfolderName(name, unit, item.Item, "Percent"), item.Value.Count, this.lastTimestamp);
                Send(SubfolderName(name, AsRate(unit, rateUnit), item.Item, "Rate-Mean"), item.Value.MeanRate, this.lastTimestamp);
                Send(SubfolderName(name, AsRate(unit, rateUnit), item.Item, "Rate-1-min"), item.Value.OneMinuteRate, this.lastTimestamp);
                Send(SubfolderName(name, AsRate(unit, rateUnit), item.Item, "Rate-5-min"), item.Value.FiveMinuteRate, this.lastTimestamp);
                Send(SubfolderName(name, AsRate(unit, rateUnit), item.Item, "Rate-15-min"), item.Value.FifteenMinuteRate, this.lastTimestamp);
            }
        }

        protected override void ReportHistogram(string name, HistogramValue value, Unit unit, MetricTags tags)
        {
            Send(SubfolderName(name, unit, "Count"), value.Count, this.lastTimestamp);
            Send(SubfolderName(name, unit, "Last"), value.LastValue, this.lastTimestamp);
            Send(SubfolderName(name, unit, "Min"), value.Min, this.lastTimestamp);
            Send(SubfolderName(name, unit, "Mean"), value.Mean, this.lastTimestamp);
            Send(SubfolderName(name, unit, "Max"), value.Max, this.lastTimestamp);
            Send(SubfolderName(name, unit, "StdDev"), value.StdDev, this.lastTimestamp);
            Send(SubfolderName(name, unit, "Median"), value.Median, this.lastTimestamp);
            Send(SubfolderName(name, unit, "p75"), value.Percentile75, this.lastTimestamp);
            Send(SubfolderName(name, unit, "p95"), value.Percentile95, this.lastTimestamp);
            Send(SubfolderName(name, unit, "p98"), value.Percentile98, this.lastTimestamp);
            Send(SubfolderName(name, unit, "p99"), value.Percentile99, this.lastTimestamp);
            Send(SubfolderName(name, unit, "p99,9"), value.Percentile999, this.lastTimestamp);
        }

        protected override void ReportTimer(string name, TimerValue value, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags)
        {
            Send(SubfolderName(name, unit, "Count"), value.Rate.Count, this.lastTimestamp);
            Send(SubfolderName(name, unit, "Active_Sessions"), value.ActiveSessions, this.lastTimestamp);

            Send(SubfolderName(name, AsRate(unit, rateUnit), "Rate-Mean"), value.Rate.MeanRate, this.lastTimestamp);
            Send(SubfolderName(name, AsRate(unit, rateUnit), "Rate-1-min"), value.Rate.OneMinuteRate, this.lastTimestamp);
            Send(SubfolderName(name, AsRate(unit, rateUnit), "Rate-5-min"), value.Rate.FiveMinuteRate, this.lastTimestamp);
            Send(SubfolderName(name, AsRate(unit, rateUnit), "Rate-15-min"), value.Rate.FifteenMinuteRate, this.lastTimestamp);

            Send(SubfolderName(name, durationUnit.Unit(), "Duration-Last"), value.Histogram.LastValue, this.lastTimestamp);
            Send(SubfolderName(name, durationUnit.Unit(), "Duration-Min"), value.Histogram.Min, this.lastTimestamp);
            Send(SubfolderName(name, durationUnit.Unit(), "Duration-Mean"), value.Histogram.Mean, this.lastTimestamp);
            Send(SubfolderName(name, durationUnit.Unit(), "Duration-Max"), value.Histogram.Max, this.lastTimestamp);
            Send(SubfolderName(name, durationUnit.Unit(), "Duration-StdDev"), value.Histogram.StdDev, this.lastTimestamp);
            Send(SubfolderName(name, durationUnit.Unit(), "Duration-Median"), value.Histogram.Median, this.lastTimestamp);
            Send(SubfolderName(name, durationUnit.Unit(), "Duration-p75"), value.Histogram.Percentile75, this.lastTimestamp);
            Send(SubfolderName(name, durationUnit.Unit(), "Duration-p95"), value.Histogram.Percentile95, this.lastTimestamp);
            Send(SubfolderName(name, durationUnit.Unit(), "Duration-p98"), value.Histogram.Percentile98, this.lastTimestamp);
            Send(SubfolderName(name, durationUnit.Unit(), "Duration-p99"), value.Histogram.Percentile99, this.lastTimestamp);
            Send(SubfolderName(name, durationUnit.Unit(), "Duration-p999"), value.Histogram.Percentile999, this.lastTimestamp);
        }

        protected override void ReportHealth(HealthStatus status)
        { }

        protected virtual void Send(string name, long value, DateTime timestamp)
        {
            Send(name, value.ToString("D", CultureInfo.InvariantCulture), timestamp);
        }

        protected virtual void Send(string name, double value, DateTime timestamp)
        {
            Send(name, value.ToString("F", CultureInfo.InvariantCulture), timestamp);
        }

        protected virtual void Send(string name, string value, DateTime timestamp)
        {
            Send(name, value, timestamp.ToUnixTime().ToString("D", CultureInfo.InvariantCulture));
        }

        protected virtual void Send(string name, string value, string timestamp)
        {
            this.sender.Send(name, value, timestamp);
        }

        protected override string FormatContextName(IEnumerable<string> contextStack, string contextName)
        {
            var parts = contextStack.Concat(new[] { contextName })
                .Select(c => GraphiteName(c));

            return string.Join(".", parts);
        }

        protected override string FormatMetricName<T>(string context, MetricValueSource<T> metric)
        {
            return string.Concat(context, ".", GraphiteName(metric.Name));
        }


        protected virtual string AsRate(Unit unit, TimeUnit rateUnit)
        {
            return string.Concat(unit.Name, "-per-", rateUnit.Unit());
        }

        protected virtual string SubfolderName(string cleanName, Unit unit, string itemName, string itemSuffix)
        {
            return Name(string.Concat(cleanName, ".", GraphiteName(itemName), "-", GraphiteName(itemSuffix)), unit);
        }

        protected virtual string SubfolderName(string cleanName, Unit unit, string itemSuffix)
        {
            return Name(string.Concat(cleanName, ".", GraphiteName(itemSuffix)), unit);
        }

        protected virtual string Name(string cleanName, Unit unit, string itemSuffix)
        {
            return Name(string.Concat(cleanName, "-", GraphiteName(itemSuffix)), unit);
        }


        protected virtual string Name(string cleanName, Unit unit)
        {
            return Name(cleanName, unit.Name);
        }

        protected virtual string Name(string cleanName, string unit)
        {
            return string.Concat(cleanName, FormatUnit(unit, cleanName));
        }

        protected virtual string FormatUnit(string unit, string name)
        {
            if (string.IsNullOrEmpty(unit))
            {
                return string.Empty;
            }
            var clean = GraphiteName(unit);

            if (string.IsNullOrEmpty(clean))
            {
                return string.Empty;
            }

            if (name.EndsWith(clean, StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Empty;
            }

            return string.Concat("-", clean);
        }

        protected virtual string GraphiteName(string name)
        {
            var noSlash = slash.Replace(name, "-per-");
            return invalid.Replace(noSlash, "_").Trim('_');
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                using (this.sender) { }
            }
        }
    }
}

