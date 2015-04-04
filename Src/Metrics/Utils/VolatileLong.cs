using System.Threading;

namespace Metrics.Utils
{
    internal struct VolatileLong
    {
        private long value;

        public VolatileLong(long value)
            : this()
        {
            this.value = value;
        }

        public void Set(long value)
        {
            Thread.VolatileWrite(ref this.value, value);
        }

        public long Get()
        {
            return Thread.VolatileRead(ref this.value);
        }
    }
}
