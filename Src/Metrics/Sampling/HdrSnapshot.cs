using System.Collections.Generic;
using System.Linq;
using HdrHistogram;

namespace Metrics.Sampling
{
    internal sealed class HdrSnapshot : Snapshot
    {
        private readonly AbstractHistogram histogram;
        private readonly string minUserValue;
        private readonly string maxUserValue;


        public HdrSnapshot(AbstractHistogram histogram, string minUserValue, string maxUserValue)
        {
            this.histogram = histogram;
            this.minUserValue = minUserValue;
            this.maxUserValue = maxUserValue;
        }

        public IEnumerable<long> Values
        {
            get { return this.histogram.RecordedValues().Select(v => v.getValueIteratedTo()); }
        }

        public double GetValue(double quantile)
        {
            return this.histogram.getValueAtPercentile(quantile * 100);
        }

        public long Count { get { return this.histogram.getTotalCount(); } }
        public long Max { get { return this.histogram.getMaxValue(); } }
        public string MaxUserValue { get { return this.maxUserValue; } }
        public double Mean { get { return this.histogram.getMean(); } }
        public double Median { get { return this.histogram.getValueAtPercentile(50); } }
        public long Min { get { return this.histogram.getMinValue(); } }
        public string MinUserValue { get { return this.minUserValue; } }
        public double Percentile75 { get { return this.histogram.getValueAtPercentile(75); } }
        public double Percentile95 { get { return this.histogram.getValueAtPercentile(95); } }
        public double Percentile98 { get { return this.histogram.getValueAtPercentile(98); } }
        public double Percentile99 { get { return this.histogram.getValueAtPercentile(99); } }
        public double Percentile999 { get { return this.histogram.getValueAtPercentile(99.9); } }
        public double StdDev { get { return this.histogram.getStdDeviation(); } }
        public int Size { get { return this.histogram.getEstimatedFootprintInBytes(); } }
    }
}