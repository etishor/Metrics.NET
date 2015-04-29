using System;
using HdrHistogram;
using Metrics.ConcurrencyUtilities;

namespace Metrics.Sampling
{
    public sealed class HdrHistogramReservoir : Reservoir
    {
        private readonly Recorder recorder;

        private readonly HdrHistogram.Histogram runningTotals;
        private HdrHistogram.Histogram intervalHistogram;

        private AtomicLong maxValue = new AtomicLong(0);
        private string maxUserValue = null;
        private readonly object maxValueLock = new object();

        private AtomicLong minValue = new AtomicLong(long.MaxValue);
        private string minUserValue = null;
        private readonly object minValueLock = new object();

        public HdrHistogramReservoir()
            : this(new Recorder(2))
        { }

        public HdrHistogramReservoir(Recorder recorder)
        {
            this.recorder = recorder;

            this.intervalHistogram = recorder.GetIntervalHistogram();
            this.runningTotals = new HdrHistogram.Histogram(this.intervalHistogram.NumberOfSignificantValueDigits);
        }

        public void Update(long value, string userValue = null)
        {
            this.recorder.RecordValue(value);
            if (userValue != null)
            {
                TrackMinMaxUserValue(value, userValue);
            }
        }

        public Snapshot GetSnapshot(bool resetReservoir = false)
        {
            return new HdrSnapshot(UpdateTotals(), this.minUserValue, this.maxUserValue);
        }

        public void Reset()
        {
            this.recorder.Reset();
            this.runningTotals.reset();
            this.intervalHistogram.reset();
        }

        private HdrHistogram.Histogram UpdateTotals()
        {
            lock (this.runningTotals)
            {
                this.intervalHistogram = this.recorder.GetIntervalHistogram(this.intervalHistogram);
                this.runningTotals.add(this.intervalHistogram);
                return this.runningTotals.copy() as HdrHistogram.Histogram;
            }
        }

        private void TrackMinMaxUserValue(long value, string userValue)
        {
            if (value > this.maxValue.NonVolatileGetValue())
            {
                SetMaxValue(value, userValue);
            }

            if (value < this.minValue.NonVolatileGetValue())
            {
                SetMinValue(value, userValue);
            }
        }

        private void SetMaxValue(long value, string userValue)
        {
            long current;
            while (value > (current = this.maxValue.GetValue()))
            {
                this.maxValue.CompareAndSwap(current, value);
            }

            if (value == current)
            {
                lock (this.maxValueLock)
                {
                    if (value == this.maxValue.GetValue())
                    {
                        this.maxUserValue = userValue;
                    }
                }
            }
        }

        private void SetMinValue(long value, string userValue)
        {
            long current;
            while (value < (current = this.minValue.GetValue()))
            {
                this.minValue.CompareAndSwap(current, value);
            }

            if (value == current)
            {
                lock (this.minValueLock)
                {
                    if (value == this.minValue.GetValue())
                    {
                        this.minUserValue = userValue;
                    }
                }
            }
        }

        private class HdrSnapshot : Snapshot
        {
            public HdrSnapshot(AbstractHistogram histogram, string minUserValue, string maxUserValue)
            {
                Count = histogram.getTotalCount();
                Max = histogram.getMaxValue();
                MaxUserValue = maxUserValue;
                Mean = histogram.getMean();
                Median = Percentile(histogram, 0.5);
                Min = histogram.getMinValue();
                MinUserValue = minUserValue;

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

        public long Count
        {
            get { throw new NotImplementedException(); }
        }

        public int Size
        {
            get { throw new NotImplementedException(); }
        }

        public bool Merge(Reservoir reservoir)
        {
            throw new NotImplementedException();
        }
    }
}
