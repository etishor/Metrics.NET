using Metrics.Utils;

namespace Metrics.Core
{
    public class HistogramMetric : Histogram
    {
        private readonly Reservoir reservoir;
        private readonly AtomicLong counter = new AtomicLong();

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
            this.counter.Increment();
            this.reservoir.Update(value);
        }

        public HistogramValue Value
        {
            get
            {
                return new HistogramValue(this.counter.Value, this.Snapshot);
            }
        }

        private static Reservoir SamplingTypeToReservoir(SamplingType samplingType)
        {
            switch (samplingType)
            {
                case SamplingType.FavourRecent: return new ExponentiallyDecayingReservoir();
                case SamplingType.LongTerm: return new UniformReservoir();
                case SamplingType.SlidingWindow: return new SlidingWindowReservoir();
            }
            throw new System.NotImplementedException();
        }
    }
}
