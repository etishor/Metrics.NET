using Metrics.Reporters;

namespace Metrics.NLog
{
    using System;
    using System.IO;
    using Metrics.MetricData;
    using LogEventInfo = global::NLog.LogEventInfo;
    using LogLevel = global::NLog.LogLevel;
    using LogManager = global::NLog.LogManager;

    public class NLogTextReporter : HumanReadableReport
    {
        private string metricType = null;
        private string metricName = null;

        protected override void StartMetricGroup(string metricType, DateTime timestamp)
        {
            this.metricType = metricType;
            base.StartMetricGroup(metricType, timestamp);
        }

        protected override void ReportGauge(string name, double value, Unit unit, MetricTags tags)
        {
            this.metricName = name;
            base.ReportGauge(name, value, unit, tags);
        }

        protected override void ReportCounter(string name, CounterValue value, Unit unit, MetricTags tags)
        {
            this.metricName = name;
            base.ReportCounter(name, value, unit, tags);
        }

        protected override void ReportMeter(string name, MeterValue value, Unit unit, TimeUnit rateUnit, MetricTags tags)
        {
            this.metricName = name;
            base.ReportMeter(name, value, unit, rateUnit, tags);
        }

        protected override void ReportHistogram(string name, HistogramValue value, Unit unit, MetricTags tags)
        {
            this.metricName = name;
            base.ReportHistogram(name, value, unit, tags);
        }

        protected override void ReportTimer(string name, TimerValue value, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags)
        {
            this.metricName = name;
            base.ReportTimer(name, value, unit, rateUnit, durationUnit, tags);
        }

        protected override void WriteLine(string line, params string[] args)
        {
            if (this.metricType == null || this.metricName == null)
            {
                return;
            }

            var loggerName = string.Format("Metrics.Text.{0}.{1}", this.metricType, this.metricName);
            var logEvent = LogEventInfo.Create(LogLevel.Info, loggerName, string.Format(line, args));
            logEvent.Properties.Add("MetricType", CleanFileName(metricType));
            logEvent.Properties.Add("MetricName", CleanFileName(metricName));
            LogManager.GetLogger(loggerName).Log(logEvent);
        }

        protected virtual string CleanFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            foreach (var c in invalid)
            {
                name = name.Replace(c, '_');
            }
            return name;
        }
    }
}
