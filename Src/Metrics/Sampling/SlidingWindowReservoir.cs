using System;
using System.Linq;
using Metrics.Utils;

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
            var count = this.count.Increment();
            this.values[(int)((count - 1) % values.Length)] = new UserValueWrapper(value, userValue);
        }

        public void Reset()
        {
            Array.Clear(this.values, 0, values.Length);
            count.SetValue(0L);
        }

        public long Count { get { return this.count.Value; } }
        public int Size { get { return Math.Min((int)this.count.Value, values.Length); } }

        public Snapshot GetSnapshot(bool resetReservoir = false)
        {
            var size = this.Size;
            if (size == 0)
            {
                return new UniformSnapshot(0, Enumerable.Empty<long>());
            }

            UserValueWrapper[] values = new UserValueWrapper[size];
            Array.Copy(this.values, values, size);

            if (resetReservoir)
            {
                Array.Clear(this.values, 0, values.Length);
                count.SetValue(0L);
            }

            Array.Sort(values, UserValueWrapper.Comparer);
            var minValue = values[0].UserValue;
            var maxValue = values[size - 1].UserValue;
            return new UniformSnapshot(this.count.Value, values.Select(v => v.Value), valuesAreSorted: true, minUserValue: minValue, maxUserValue: maxValue);
        }
    }
}
