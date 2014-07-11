using System;
using System.Globalization;
using Metrics.Reporters;
using System.IO;
using log4net;
using log4net.Core;

namespace Metrics.Log4Net
{

    public class Log4NetTextReporter : HumanReadableReporter
    {
        private string metricType = null;
        private string metricName = null;

        protected override void StartMetricGroup(string metricType)
        {
            this.metricType = metricType;
            base.StartMetricGroup(metricType);
        }

        protected override void ReportGauge(string name, double value, Unit unit)
        {
            this.metricName = name;
            base.ReportGauge(name, value, unit);
        }

        protected override void ReportCounter(string name, long value, Unit unit)
        {
            this.metricName = name;
            base.ReportCounter(name, value, unit);
        }

        protected override void ReportMeter(string name, MeterValue value, Unit unit, TimeUnit rateUnit)
        {
            this.metricName = name;
            base.ReportMeter(name, value, unit, rateUnit);
        }

        protected override void ReportHistogram(string name, HistogramValue value, Unit unit)
        {
            this.metricName = name;
            base.ReportHistogram(name, value, unit);
        }

        protected override void ReportTimer(string name, TimerValue value, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit)
        {
            this.metricName = name;
            base.ReportTimer(name, value, unit, rateUnit, durationUnit);
        }

        protected override void WriteLine(string line, params string[] args)
        {
            if (this.metricType == null || this.metricName == null)
            {
                return;
            }

            var loggerName = string.Format(CultureInfo.InvariantCulture, "Metrics.Text.{0}.{1}", this.metricType, this.metricName);
            var logEvent = new LoggingEvent(new LoggingEventData { Level = Level.Info, LoggerName = loggerName, Message = string.Format(line, args), TimeStamp = DateTime.Now});
            logEvent.Properties["MetricType"] = CleanFileName(metricType);
            logEvent.Properties["MetricName"] = CleanFileName(metricName);
            LogManager.GetLogger(loggerName).Logger.Log(logEvent);
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
