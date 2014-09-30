using Metrics.Sampling;
using Metrics.Utils;

namespace Metrics
{
    /// <summary>
    /// A Histogram measures the distribution of values in a stream of data: e.g., the number of results returned by a search.
    /// </summary>
    public interface Histogram : ResetableMetric, Utils.IHideObjectMembers
    {
        /// <summary>
        /// Records a value.
        /// </summary>
        /// <param name="value">Value to be added to the histogram.</param>
        /// <param name="userValue">A custom user value that will be associated to the results.
        /// Useful for tracking (for example) for which id the max or min value was recorded.
        /// </param>
        void Update(long value, string userValue = null);
    }

    /// <summary>
    /// The value reported by a Histogram Metric
    /// </summary>
    public struct HistogramValue
    {
        public readonly long Count;

        public readonly double LastValue;
        public readonly string LastUserValue;
        public readonly double Max;
        public readonly string MaxUserValue;
        public readonly double Mean;
        public readonly double Min;
        public readonly string MinUserValue;
        public readonly double StdDev;
        public readonly double Median;
        public readonly double Percentile75;
        public readonly double Percentile95;
        public readonly double Percentile98;
        public readonly double Percentile99;
        public readonly double Percentile999;
        public readonly int SampleSize;

        public HistogramValue(long count, double lastValue, string lastUserValue, Snapshot snapshot)
            : this(count,
            lastValue,
            lastUserValue,
            snapshot.Max,
            snapshot.MaxUserValue,
            snapshot.Mean,
            snapshot.Min,
            snapshot.MinUserValue,
            snapshot.StdDev,
            snapshot.Median,
            snapshot.Percentile75,
            snapshot.Percentile95,
            snapshot.Percentile98,
            snapshot.Percentile99,
            snapshot.Percentile999,
            snapshot.Size)
        { }

        public HistogramValue(long count,
            double lastValue,
            string lastUserValue,
            double max,
            string maxUserValue,
            double mean,
            double min,
            string minUserValue,
            double stdDev,
            double median,
            double percentile75,
            double percentile95,
            double percentile98,
            double percentile99,
            double percentile999,
            int sampleSize)
        {
            this.Count = count;
            this.LastValue = lastValue;
            this.LastUserValue = lastUserValue;
            this.Max = max;
            this.MaxUserValue = maxUserValue;
            this.Mean = mean;
            this.Min = min;
            this.MinUserValue = minUserValue;
            this.StdDev = stdDev;
            this.Median = median;
            this.Percentile75 = percentile75;
            this.Percentile95 = percentile95;
            this.Percentile98 = percentile98;
            this.Percentile99 = percentile99;
            this.Percentile999 = percentile999;
            this.SampleSize = sampleSize;
        }

        public HistogramValue Scale(double factor)
        {
            return new HistogramValue(this.Count,
                this.LastValue * factor,
                this.LastUserValue,
                this.Max * factor,
                this.MaxUserValue,
                this.Mean * factor,
                this.Min * factor,
                this.MinUserValue,
                this.StdDev * factor,
                this.Median * factor,
                this.Percentile75 * factor,
                this.Percentile95 * factor,
                this.Percentile98 * factor,
                this.Percentile99 * factor,
                this.Percentile999 * factor,
                this.SampleSize);
        }

        public HistogramValue Scale(TimeUnit durationUnit)
        {
            return this.Scale(1.0 / (double)durationUnit.ToNanoseconds(1));
        }
    }

    /// <summary>
    /// Combines the value of the histogram with the defined unit for the value.
    /// </summary>
    public sealed class HistogramValueSource : MetricValueSource<HistogramValue>
    {
        public HistogramValueSource(string name, MetricValueProvider<HistogramValue> value, Unit unit, MetricTags tags)
            : base(name, value, unit, tags)
        { }
    }
}
