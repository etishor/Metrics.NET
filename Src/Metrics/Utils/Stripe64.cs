using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Metrics.Utils
{
    public abstract class Stripe64
    {
        [StructLayout(LayoutKind.Explicit, Size = 64 * 2)]
        protected class Cell
        {
            [FieldOffset(64)]
            private long value;

            public Cell(long x)
            {
                value = x;
            }

            public long Value
            {
                get { return Thread.VolatileRead(ref this.value); }
                set { Thread.VolatileWrite(ref this.value, value); }
            }

            public bool Cas(long cmp, long val)
            {
                return Interlocked.CompareExchange(ref value, val, cmp) == cmp;
            }
        }

        private static readonly int NumberOfCpus = Environment.ProcessorCount;

        protected volatile Cell[] cells;
        private long @base;
        private int cellsBusy;

        protected long Base
        {
            get { return Thread.VolatileRead(ref this.@base); }
            set { Thread.VolatileWrite(ref this.@base, value); }
        }

        protected bool CompareAndSwapBase(long cmp, long val)
        {
            return Interlocked.CompareExchange(ref @base, val, cmp) == cmp;
        }

        private bool CasCellsBusy()
        {
            return Interlocked.CompareExchange(ref cellsBusy, 1, 0) == 0;
        }

        protected void LongAccumulate(long x, bool wasUncontended)
        {
            int h = GetProbe();

            bool collide = false;                // True if last slot nonempty
            for (; ; )
            {
                Cell[] @as; Cell a; int n; long v;
                if ((@as = cells) != null && (n = @as.Length) > 0)
                {
                    if ((a = @as[(n - 1) & h]) == null)
                    {
                        if (cellsBusy == 0)
                        {       // Try to attach new Cell
                            Cell r = new Cell(x);   // Optimistically create
                            if (cellsBusy == 0 && CasCellsBusy())
                            {
                                bool created = false;
                                try
                                {               // Recheck under lock
                                    Cell[] rs; int m, j;
                                    if ((rs = cells) != null &&
                                        (m = rs.Length) > 0 &&
                                        rs[j = (m - 1) & h] == null)
                                    {
                                        rs[j] = r;
                                        created = true;
                                    }
                                }
                                finally
                                {
                                    cellsBusy = 0;
                                }
                                if (created)
                                    break;
                                continue;           // Slot is now non-empty
                            }
                        }
                        collide = false;
                    }
                    else if (!wasUncontended)       // CAS already known to fail
                        wasUncontended = true;      // Continue after rehash
                    else if (a.Cas(v = a.Value, v + x))
                        break;
                    else if (n >= NumberOfCpus || cells != @as)
                        collide = false;            // At max size or stale
                    else if (!collide)
                        collide = true;
                    else if (cellsBusy == 0 && CasCellsBusy())
                    {
                        try
                        {
                            if (cells == @as)
                            {      // Expand table unless stale
                                Cell[] rs = new Cell[n << 1];
                                for (int i = 0; i < n; ++i)
                                    rs[i] = @as[i];
                                cells = rs;
                            }
                        }
                        finally
                        {
                            cellsBusy = 0;
                        }
                        collide = false;
                        continue;                   // Retry with expanded table
                    }
                    h = AdvanceProbe(h);
                }
                else if (cellsBusy == 0 && cells == @as && CasCellsBusy())
                {
                    bool init = false;
                    try
                    {                           // Initialize table
                        if (cells == @as)
                        {
                            Cell[] rs = new Cell[2];
                            rs[h & 1] = new Cell(x);
                            cells = rs;
                            init = true;
                        }
                    }
                    finally
                    {
                        cellsBusy = 0;
                    }
                    if (init)
                        break;
                }
                else if (CompareAndSwapBase(v = Base, v + x))
                    break;                          // Fall back on using base
            }
        }

        private static int AdvanceProbe(int probe)
        {
            probe ^= probe << 13;   // xorshift
            probe ^= (int)((uint)probe >> 17);
            probe ^= probe << 5;
            hashCode.Value.Code = probe;
            return probe;
        }
        protected static int GetProbe()
        {
            return hashCode.Value.Code;
        }

        private static readonly ThreadLocal<ThreadHashCode> hashCode = new ThreadLocal<ThreadHashCode>(() => new ThreadHashCode());
        private class ThreadHashCode
        {
            public ThreadHashCode()
            {
                this.Code = ThreadLocalRandom.Random.Next(1, int.MaxValue);
            }

            public int Code { get; set; }
        }
    }
}
