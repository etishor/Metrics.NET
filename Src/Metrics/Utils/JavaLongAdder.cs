namespace Metrics.Utils
{
    public class JavaLongAdder : JavaStripe64
    {
        public JavaLongAdder()
        { }
        public JavaLongAdder(long value)
        {
            Add(value);
        }

        public long Value
        {
            get { return Sum(); }
        }

        public void SetValue(long value)
        {
            Reset();
            Add(value);
        }


        public long GetAndReset()
        {
            return SumThenReset();
        }

        public long GetAndSet(long value)
        {
            var sum = SumThenReset();
            Add(value);
            return sum;
        }

        private void Reset()
        {
            Cell[] @as = this.cells; Cell a;
            this.Base = 0L;
            if (@as != null)
            {
                for (int i = 0; i < @as.Length; ++i)
                {
                    if ((a = @as[i]) != null)
                        a.Value = 0L;
                }
            }
        }

        private long SumThenReset()
        {
            Cell[] @as = this.cells; Cell a;
            long sum = this.Base;
            this.Base = 0L;
            if (@as != null)
            {
                for (int i = 0; i < @as.Length; ++i)
                {
                    if ((a = @as[i]) != null)
                    {
                        sum += a.Value;
                        a.Value = 0L;
                    }
                }
            }
            return sum;
        }

        public void Add(long x)
        {
            Cell[] @as;
            long b, v;
            int m;
            Cell a;
            if ((@as = this.cells) != null || !CompareAndSwapBase(b = this.Base, b + x))
            {
                var uncontended = true;
                if (@as == null || (m = @as.Length - 1) < 0 || (a = @as[GetProbe() & m]) == null || !(uncontended = a.Cas(v = a.Value, v + x)))
                {
                    LongAccumulate(x, uncontended);
                }
            }
        }

        public void Increment()
        {
            Add(1L);
        }

        public long IncrementAndGet()
        {
            Add(1L);
            return Sum();
        }

        public void Decrement()
        {
            Add(-1L);
        }

        private long Sum()
        {
            Cell[] @as = this.cells; Cell a;
            long sum = this.Base;
            if (@as != null)
            {
                for (int i = 0; i < @as.Length; ++i)
                {
                    if ((a = @as[i]) != null)
                        sum += a.Value;
                }
            }
            return sum;
        }

        // FIXME: this will probably not be needed. Current version is broken
        internal bool CompareAndSet(long workingUc, long newUncounted)
        {
            SetValue(newUncounted);
            return true;
        }
    }
}