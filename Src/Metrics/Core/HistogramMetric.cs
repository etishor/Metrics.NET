using System;
using Metrics.Sampling;
using Metrics.Utils;

namespace Metrics.Core
{
    public interface HistogramImplementation : Histogram, MetricValueProvider<HistogramValue> { }

    public sealed class HistogramMetric : HistogramImplementation
    {
        private readonly Reservoir reservoir;
        private AtomicLong counter = new AtomicLong();
        private UserValueWrapper last = new UserValueWrapper();

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

        public void Update(long value, string userValue = null)
        {
            this.last = new UserValueWrapper(value, userValue);

            this.counter.Increment();
            this.reservoir.Update(value, userValue);
        }

        public HistogramValue Value
        {
            get
            {
                return new HistogramValue(this.counter.Value, this.last.Value, this.last.UserValue, this.Snapshot);
            }
        }

        public void Reset()
        {
            this.last = new UserValueWrapper();
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
