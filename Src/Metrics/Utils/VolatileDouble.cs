using System.Threading;

namespace Metrics.Utils
{
    internal struct VolatileDouble
    {
        private double value;

        public VolatileDouble(double value)
            : this()
        {
            this.value = value;
        }

        public void Set(double value)
        {
            Thread.VolatileWrite(ref this.value, value);
        }

        public double Get()
        {
            return Thread.VolatileRead(ref this.value);
        }
    }
}
