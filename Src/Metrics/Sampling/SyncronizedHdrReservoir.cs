using HdrHistogram;

namespace Metrics.Sampling
{
    public sealed class SyncronizedHdrReservoir : Reservoir
    {
        private readonly AbstractHistogram histogram;
        private readonly object padlock = new object();

        internal SyncronizedHdrReservoir(AbstractHistogram histogram)
        {
            this.histogram = histogram;
        }

        public SyncronizedHdrReservoir(int numberOfSignificantValueDigits)
            : this(new HdrHistogram.Histogram(numberOfSignificantValueDigits))
        { }

        public SyncronizedHdrReservoir()
            : this(2)
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
            lock (this.padlock) this.histogram.RecordValue(value);
        }

        public Snapshot GetSnapshot(bool resetReservoir = false)
        {
            lock (this.padlock) return new HdrSnapshot(this.histogram, null, null);
        }

        public void Reset()
        {
            lock (this.padlock) this.histogram.reset();
        }
    }
}
