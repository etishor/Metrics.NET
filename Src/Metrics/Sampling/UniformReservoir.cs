
using System;
using System.Linq;
using Metrics.ConcurrencyUtilities;

namespace Metrics.Sampling
{
    public sealed class UniformReservoir : Reservoir
    {
        private const int DefaultSize = 1028;
        private const int BitsPerLong = 63;

        private AtomicLong count = new AtomicLong();

        private readonly UserValueWrapper[] values;

        public UniformReservoir()
            : this(DefaultSize)
        { }

        public UniformReservoir(int size)
        {
            this.values = new UserValueWrapper[size];
        }

        public long Count { get { return this.count.GetValue(); } }

        public int Size
        {
            get
            {
                return Math.Min((int)this.count.GetValue(), this.values.Length);
            }
        }

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
                count.SetValue(0L);
            }

            Array.Sort(snapshotValues, UserValueWrapper.Comparer);
            var minValue = snapshotValues[0].UserValue;
            var maxValue = snapshotValues[size - 1].UserValue;
            return new UniformSnapshot(this.count.GetValue(), snapshotValues.Select(v => new Tuple<long, string>(v.Value, v.UserValue)), valuesAreSorted: true, minUserValue: minValue, maxUserValue: maxValue);
        }

        public void Update(long value, string userValue = null)
        {
            var c = this.count.Increment();
            if (c <= this.values.Length)
            {
                this.values[(int)c - 1] = new UserValueWrapper(value, userValue);
            }
            else
            {
                var r = NextLong(c);
                if (r < this.values.Length)
                {
                    this.values[(int)r] = new UserValueWrapper(value, userValue);
                }
            }
        }

        public void Reset()
        {
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

        private static long NextLong(long max)
        {
            long bits, val;
            do
            {
                bits = ThreadLocalRandom.NextLong() & (~(1L << BitsPerLong));
                val = bits % max;
            } while (bits - val + (max - 1) < 0L);
            return val;
        }
    }
}
