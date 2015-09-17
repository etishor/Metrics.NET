﻿
using Metrics.ConcurrencyUtilities;
using System;
using System.Linq;
using static System.Math;

namespace Metrics.Sampling
{
    public sealed class UniformReservoir : Reservoir
    {
        private const int DefaultSize = 1028;

        private AtomicLong count = new AtomicLong();

        private readonly UserValueWrapper[] values;

        public UniformReservoir()
            : this(DefaultSize)
        { }

        public UniformReservoir(int size)
        {
            this.values = new UserValueWrapper[size];
        }

        public int Size => Min((int)this.count.GetValue(), this.values.Length);

        public Snapshot GetSnapshot(bool resetReservoir = false)
        {
            var size = Size;
            if (size == 0)
            {
                return new UniformSnapshot(0, Enumerable.Empty<long>());
            }

            var snapshotValues = new UserValueWrapper[size];
            Array.Copy(this.values, snapshotValues, size);

            if (resetReservoir)
            {
                this.count.SetValue(0L);
            }

            Array.Sort(snapshotValues, UserValueWrapper.Comparer);
            var minValue = snapshotValues[0].UserValue;
            var maxValue = snapshotValues[size - 1].UserValue;
            return new UniformSnapshot(this.count.GetValue(), snapshotValues.Select(v => v.Value), valuesAreSorted: true, minUserValue: minValue, maxUserValue: maxValue);
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
                var r = ThreadLocalRandom.NextLong(c);
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
    }
}
