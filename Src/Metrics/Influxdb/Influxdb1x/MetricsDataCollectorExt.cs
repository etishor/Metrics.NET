using System.Collections.Generic;

namespace Metrics.NET.InfluxDB
{
    /// <summary>
    /// Extension methods to faciltate collecting data from Metrics.NET
    /// </summary>
    public static class MetricsDataCollectorExt
    {
        /// <summary>
        /// Merge a Meter's metrics into a map of name/values
        /// </summary>
        /// <param name="meter"></param>
        /// <param name="values"></param>
        public static void AddMeterValues(this MetricData.MeterValue meter, IDictionary<string, object> values)
        {
            values.Add("count.meter", meter.Count);
            values.Add("rate1m", meter.OneMinuteRate);
            values.Add("rate5m", meter.FiveMinuteRate);
            values.Add("rate15m", meter.FifteenMinuteRate);
            values.Add("rate.mean", meter.MeanRate);
        }

        /// <summary>
        /// Merge a Histogram's metrics into a map of name/values
        /// </summary>
        /// <param name="hist"></param>
        /// <param name="values"></param>
        public static void AddHistogramValues(this MetricData.HistogramValue hist, IDictionary<string, object> values)
        {
            values.Add("samples", hist.SampleSize);
            values.Add("last", hist.LastValue);
            values.Add("count.hist", hist.Count);
            values.Add("min", hist.Min);
            values.Add("max", hist.Max);
            values.Add("mean", hist.Mean);
            values.Add("median", hist.Median);
            values.Add("stddev", hist.StdDev);
            values.Add("p999", hist.Percentile999);
            values.Add("p99", hist.Percentile99);
            values.Add("p98", hist.Percentile98);
            values.Add("p95", hist.Percentile95);
            values.Add("p75", hist.Percentile75);

            if (hist.LastUserValue != null)
            {
                values.Add("user.last", hist.LastUserValue);
            }

            if (hist.MinUserValue != null)
            {
                values.Add("user.min", hist.MinUserValue);
            }

            if (hist.MaxUserValue != null)
            {
                values.Add("user.max", hist.MaxUserValue);
            }
        }
    }
}
