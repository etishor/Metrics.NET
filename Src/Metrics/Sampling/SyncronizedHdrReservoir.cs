using System;
using HdrHistogram.NET;
using Metrics.Utils;

namespace Metrics.Sampling
{
    public sealed class SyncronizedHdrReservoir : Reservoir
    {
        private readonly AbstractHistogram histogram;
        private readonly object padlock = new object();

        public SyncronizedHdrReservoir(AbstractHistogram histogram)
        {
            this.histogram = histogram;
        }

        public SyncronizedHdrReservoir(long highestTrackableValue, int numberOfSignificantValueDigits)
            : this(new HdrHistogram.NET.Histogram(highestTrackableValue, numberOfSignificantValueDigits))
        { }

        public SyncronizedHdrReservoir()
            : this(TimeUnit.Hours.Convert(TimeUnit.Nanoseconds, 1), 3)
        { }


        public long Count
        {
            get { lock (this.padlock) return this.histogram.getTotalCount(); }
        }

        public int Size
        {
            get { lock (this.padlock) return this.histogram._getEstimatedFootprintInBytes(); }
        }

        public void Update(long value, string userValue = null)
        {
            lock (this.padlock) this.histogram.recordValue(value);
        }

        public Snapshot GetSnapshot(bool resetReservoir = false)
        {
            lock (this.padlock) return new HdrSnapshot(this.histogram);
        }

        public void Reset()
        {
            lock (this.padlock) this.histogram.reset();
        }

        public bool Merge(Reservoir reservoir)
        {
            throw new NotSupportedException("Merging is not supported for hdr histogram");
        }

        private class HdrSnapshot : Snapshot
        {
            public HdrSnapshot(AbstractHistogram histogram)
            {
                Count = histogram.getTotalCount();
                Max = histogram.getMaxValue();
                Mean = histogram.getMean();
                Median = Percentile(histogram, 0.5);
                Min = histogram.getMinValue();

                Percentile75 = Percentile(histogram, 0.75);
                Percentile95 = Percentile(histogram, 0.95);
                Percentile98 = Percentile(histogram, 0.98);
                Percentile99 = Percentile(histogram, 0.99);
                Percentile999 = Percentile(histogram, 0.999);

                Size = histogram._getEstimatedFootprintInBytes();
                StdDev = histogram.getStdDeviation();

            }

            private static double Percentile(AbstractHistogram histogram, double quantile)
            {
                return histogram.getValueAtPercentile(quantile * 100);
            }

            public double GetValue(double quantile)
            {
                throw new NotSupportedException("Only fixes quantiles are supported for now");
            }

            public long Count { get; private set; }
            public long Max { get; private set; }
            public string MaxUserValue { get; private set; }
            public double Mean { get; private set; }
            public double Median { get; private set; }
            public long Min { get; private set; }
            public string MinUserValue { get; private set; }
            public double Percentile75 { get; private set; }
            public double Percentile95 { get; private set; }
            public double Percentile98 { get; private set; }
            public double Percentile99 { get; private set; }
            public double Percentile999 { get; private set; }
            public int Size { get; private set; }
            public double StdDev { get; private set; }

            public System.Collections.Generic.IEnumerable<Tuple<long, string>> Values
            {
                get
                {
                    throw new NotSupportedException("Getting individual values is not supported");
                }
            }
        }
    }
}
