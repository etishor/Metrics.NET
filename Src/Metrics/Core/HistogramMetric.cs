using System;
using Metrics.Utils;

namespace Metrics.Core
{
    public sealed class HistogramMetric : Histogram, MetricValueProvider<HistogramValue>
    {
        private readonly Reservoir reservoir;
        private AtomicLong counter = new AtomicLong();
        private AtomicLong last = new AtomicLong();

        public HistogramMetric()
            : this(new ExponentiallyDecayingReservoir()) { }

        public HistogramMetric(SamplingType samplingType)
            : this(SamplingTypeToReservoir(samplingType)) { }

        public HistogramMetric(Reservoir reservoir)
        {
            this.reservoir = reservoir;
        }

        public long Count { get { return this.counter.Value; } }
        public Snapshot Snapshot { get { return this.reservoir.Snapshot; } }

        public void Update(long value)
        {
            this.last.SetValue(value);
            this.counter.Increment();
            this.reservoir.Update(value);
        }

        public HistogramValue Value
        {
            get
            {
                return new HistogramValue(this.counter.Value, this.last.Value, this.Snapshot);
            }
        }

        public void Reset()
        {
            this.last.SetValue(0L);
            this.counter.SetValue(0L);
            this.reservoir.Reset();
        }

        private static Reservoir SamplingTypeToReservoir(SamplingType samplingType)
        {
            switch (samplingType)
            {
                case SamplingType.FavourRecent: return new ExponentiallyDecayingReservoir();
                case SamplingType.LongTerm: return new UniformReservoir();
                case SamplingType.SlidingWindow: return new SlidingWindowReservoir();
            }
            throw new InvalidOperationException("Sampling type not implemented " + samplingType.ToString());
        }
    }
}
