using System;
using System.Linq;
using Metrics.ConcurrencyUtilities;

namespace Metrics.Sampling
{
    public sealed class SlidingWindowReservoir : Reservoir
    {
        private const int DefaultSize = 1028;

        private readonly UserValueWrapper[] values;
        private AtomicLong count = new AtomicLong();

        public SlidingWindowReservoir()
            : this(DefaultSize) { }

        public SlidingWindowReservoir(int size)
        {
            this.values = new UserValueWrapper[size];
        }

        public void Update(long value, string userValue = null)
        {
            var newCount = this.count.Increment();
            this.values[(int)((newCount - 1) % this.values.Length)] = new UserValueWrapper(value, userValue);
        }

        public void Reset()
        {
            Array.Clear(this.values, 0, this.values.Length);
            this.count.SetValue(0L);
        }

        public bool Merge(Reservoir other)
        {
            var snapshot = other.GetSnapshot();
            foreach (var value in snapshot.Values)
            {
                Update(value.Item1, value.Item2);
            }

            return true;
        }

        public long Count { get { return this.count.GetValue(); } }
        public int Size { get { return Math.Min((int)this.count.GetValue(), this.values.Length); } }

        public Snapshot GetSnapshot(bool resetReservoir = false)
        {
            var size = Size;
            if (size == 0)
            {
                return new UniformSnapshot(0, Enumerable.Empty<Tuple<long, string>>());
            }

            var snapshotValues = new UserValueWrapper[size];
            Array.Copy(this.values, snapshotValues, size);

            if (resetReservoir)
            {
                Array.Clear(this.values, 0, snapshotValues.Length);
                this.count.SetValue(0L);
            }

            Array.Sort(snapshotValues, UserValueWrapper.Comparer);
            var minValue = snapshotValues[0].UserValue;
            var maxValue = snapshotValues[size - 1].UserValue;
            return new UniformSnapshot(this.count.GetValue(), snapshotValues.Select(v => new Tuple<long, string>(v.Value, v.UserValue)), valuesAreSorted: true, minUserValue: minValue, maxUserValue: maxValue);
        }
    }
}
