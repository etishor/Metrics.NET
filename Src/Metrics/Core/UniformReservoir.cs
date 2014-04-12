
using System;
using System.Linq;
using Metrics.Utils;
namespace Metrics.Core
{
    public sealed class UniformReservoir : Reservoir
    {
        private const int DefaultSize = 1028;
        private const int BitsPerLong = 63;

        private AtomicLong count = new AtomicLong();
        private readonly AtomicLong[] values;

        public UniformReservoir()
            : this(DefaultSize)
        { }

        public UniformReservoir(int size)
        {
            this.values = new AtomicLong[size];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = new AtomicLong();
            }
        }

        public int Size
        {
            get
            {
                return Math.Min((int)this.count.Value, this.values.Length);
            }
        }

        public Snapshot Snapshot
        {
            get
            {
                return new Snapshot(this.values.Take(Size).Select(v => v.Value));
            }
        }

        public void Update(long value)
        {
            long c = this.count.Increment();
            if (c <= this.values.Length)
            {
                values[(int)c - 1].SetValue(value);
            }
            else
            {
                long r = NextLong(c);
                if (r < values.Length)
                {
                    values[(int)r].SetValue(value);
                }
            }
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
