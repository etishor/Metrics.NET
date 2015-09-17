﻿using HdrHistogram;
using Metrics.ConcurrencyUtilities;

namespace Metrics.Sampling
{
    /// <summary>
    /// Sampling reservoir based on HdrHistogram.
    /// Based on the java version from Marshall Pierce https://bitbucket.org/marshallpierce/hdrhistogram-metrics-reservoir/src/83a8ec568a1e?at=master
    /// </summary>
    public sealed class HdrHistogramReservoir : Reservoir
    {
        private readonly Recorder recorder;

        private readonly HdrHistogram.Histogram runningTotals;
        private HdrHistogram.Histogram intervalHistogram;

        private AtomicLong maxValue = new AtomicLong(0);
        private string maxUserValue;
        private readonly object maxValueLock = new object();

        private AtomicLong minValue = new AtomicLong(long.MaxValue);
        private string minUserValue;
        private readonly object minValueLock = new object();

        public HdrHistogramReservoir()
            : this(new Recorder(2))
        { }

        internal HdrHistogramReservoir(Recorder recorder)
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
            var snapshot = new HdrSnapshot(UpdateTotals(), this.minValue.GetValue(), this.minUserValue, this.maxValue.GetValue(), this.maxUserValue);
            if (resetReservoir)
            {
                this.Reset();
            }
            return snapshot;
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
    }
}
