using Metrics.Core;
using Metrics.Utils;

namespace Metrics
{
    /// <summary>
    /// A Histogram measures the distribution of values in a stream of data: e.g., the number of results returned by a search.
    /// </summary>
    public interface Histogram : Utils.IHideObjectMembers
    {
        /// <summary>
        /// Records a value.
        /// </summary>
        /// <param name="value">Value to be added to the histogram.</param>
        void Update(long value);
    }

    /// <summary>
    /// The value reported by a Histogram Metric
    /// </summary>
    public struct HistogramValue
    {
        public readonly long Count;

        public readonly double LastValue;
        public readonly double Max;
        public readonly double Mean;
        public readonly double Min;
        public readonly double StdDev;
        public readonly double Median;
        public readonly double Percentile75;
        public readonly double Percentile95;
        public readonly double Percentile98;
        public readonly double Percentile99;
        public readonly double Percentile999;
        public readonly int SampleSize;

        public HistogramValue(long count, double lastValue, Snapshot snapshot)
            : this(count, lastValue, snapshot.Max, snapshot.Mean, snapshot.Min, snapshot.StdDev,
            snapshot.Median, snapshot.Percentile75, snapshot.Percentile95, snapshot.Percentile98, snapshot.Percentile99, snapshot.Percentile999, snapshot.Size)
        { }

        public HistogramValue(long count, double lastValue, double max, double mean, double min, double stdDev,
            double median, double percentile75, double percentile95, double percentile98, double percentile99, double percentile999, int sampleSize)
        {
            this.Count = count;
            this.LastValue = lastValue;
            this.Max = max;
            this.Mean = mean;
            this.Min = min;
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
            return new HistogramValue(this.Count, this.LastValue * factor, this.Max * factor, this.Mean * factor, this.Min * factor, this.StdDev * factor,
                this.Median * factor, this.Percentile75 * factor, this.Percentile95 * factor,
                this.Percentile98 * factor, this.Percentile99 * factor, this.Percentile999 * factor, this.SampleSize);
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
        public HistogramValueSource(string name, MetricValueProvider<HistogramValue> value, Unit unit)
            : base(name, value, unit)
        { }
    }
}
