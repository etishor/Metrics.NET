
namespace Metrics.Reporters
{
    public abstract class HumanReadableReporter : Reporter
    {
        private readonly int padding;

        protected HumanReadableReporter(int padding = 20)
        {
            this.padding = padding;
        }

        protected abstract void WriteLine(string line, params string[] args);

        protected override void StartReport()
        {
            this.WriteLine("{0} - {1}", base.RegistryName, base.Timestamp.ToString());
        }

        protected override void StartMetricGroup(string metricType)
        {
            this.WriteLine();
            this.WriteLine();
            this.WriteLine("***** {0} - {1} - {2} *****", metricType, this.RegistryName, base.Timestamp.ToString());
        }

        protected void WriteMetricName(string name)
        {
            this.WriteLine();
            this.WriteLine("    {0}", name);
        }

        protected override void ReportGauge(string name, string value, Unit unit)
        {
            this.WriteMetricName(name);
            this.WriteValue("value", unit.FormatValue(value));
        }

        protected override void ReportCounter(string name, long value, Unit unit)
        {
            this.WriteMetricName(name);
            WriteValue("Count", unit.FormatCount(value));
        }

        protected override void ReportMeter(string name, MeterValue value, Unit unit, TimeUnit rateUnit)
        {
            this.WriteMetricName(name);
            this.WriteMeter(value.Scale(rateUnit), unit, rateUnit);
        }

        protected override void ReportHistogram(string name, HistogramValue value, Unit unit)
        {
            this.WriteMetricName(name);
            this.WriteHistogram(value, unit);
        }

        protected override void ReportTimer(string name, TimerValue value, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit)
        {
            this.WriteMetricName(name);
            this.WriteMeter(value.Rate.Scale(rateUnit), unit, rateUnit);
            this.WriteHistogram(value.Histogram.Scale(durationUnit), unit, durationUnit);
        }

        private void WriteMeter(MeterValue value, Unit unit, TimeUnit rateUnit)
        {
            WriteValue("Count", unit.FormatCount(value.Count));
            WriteValue("Mean Value", unit.FormatRate(value.MeanRate, rateUnit));
            WriteValue("1 Minute Rate", unit.FormatRate(value.OneMinuteRate, rateUnit));
            WriteValue("5 Minute Rate", unit.FormatRate(value.FiveMinuteRate, rateUnit));
            WriteValue("15 Minute Rate", unit.FormatRate(value.FifteenMinuteRate, rateUnit));
        }

        private void WriteHistogram(HistogramValue value, Unit unit, TimeUnit? durationUnit = null)
        {
            WriteValue("Count", unit.FormatCount(value.Count));
            WriteValue("Min", unit.FormatDuration(value.Min, durationUnit));
            WriteValue("Max", unit.FormatDuration(value.Max, durationUnit));
            WriteValue("Mean", unit.FormatDuration(value.Mean, durationUnit));
            WriteValue("StdDev", unit.FormatDuration(value.StdDev, durationUnit));
            WriteValue("Median", unit.FormatDuration(value.Median, durationUnit));
            WriteValue("75%", unit.FormatDuration(value.Percentile75, durationUnit), sign: "<=");
            WriteValue("95%", unit.FormatDuration(value.Percentile95, durationUnit), sign: "<=");
            WriteValue("98%", unit.FormatDuration(value.Percentile98, durationUnit), sign: "<=");
            WriteValue("99%", unit.FormatDuration(value.Percentile99, durationUnit), sign: "<=");
            WriteValue("99.9%", unit.FormatDuration(value.Percentile999, durationUnit), sign: "<=");
        }

        public void WriteValue(string label, string value, string sign = "=")
        {
            string pad = string.Empty;

            if (label.Length + 2 + sign.Length < padding)
            {
                pad = new string(' ', padding - label.Length - 1 - sign.Length);
            }

            this.WriteLine("{0}{1} {2} {3}", pad, label, sign, value);
        }

        private void WriteLine()
        {
            this.WriteLine(string.Empty);
        }
    }
}
