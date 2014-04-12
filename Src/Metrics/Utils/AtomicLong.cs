
using System.Runtime.InteropServices;
using System.Threading;
namespace Metrics.Utils
{
    /// <summary>
    /// Atomic long. Padded to avoid false sharing.
    /// 
    /// TBD: implement optimizations behind LongAdder from 
    /// <a href="https://github.com/dropwizard/metrics/blob/master/metrics-core/src/main/java/com/codahale/metrics/LongAdder.java">metrics-core</a>
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 64)]
    public struct AtomicLong
    {
        [FieldOffset(64)]
        private long value;

        //public AtomicLong() : this(0L) { }

        public AtomicLong(long value)
        {
            this.value = value;
        }

        public long Value
        {
            get
            {
                return Interlocked.Read(ref this.value);
            }
        }

        public void SetValue(long value)
        {
            GetAndSet(value);
        }

        public long Add(long value)
        {
            return Interlocked.Add(ref this.value, value);
        }

        public long Increment()
        {
            return Interlocked.Increment(ref this.value);
        }

        public long Decrement()
        {
            return Interlocked.Decrement(ref this.value);
        }

        public long GetAndReset()
        {
            return GetAndSet(0L);
        }

        public long GetAndSet(long value)
        {
            return Interlocked.Exchange(ref this.value, value);
        }

        public bool CompareAndSet(long expectd, long updated)
        {
            var value = Interlocked.CompareExchange(ref this.value, updated, expectd);
            return value == expectd;
        }
    }
}
