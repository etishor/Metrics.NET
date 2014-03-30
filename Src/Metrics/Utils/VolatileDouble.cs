using System.Threading;

namespace Metrics.Utils
{
    internal struct VolatileDouble
    {
        private double value;

        public static VolatileDouble operator +(VolatileDouble left, VolatileDouble right)
        {
            return Add(left, right);
        }

        private static VolatileDouble Add(VolatileDouble left, VolatileDouble right)
        {
            left.Set(left.Get() + right.Get());
            return left.Get();
        }

        public static VolatileDouble operator -(VolatileDouble left, VolatileDouble right)
        {
            left.Set(left.Get() - right.Get());
            return left.Get();
        }

        public static VolatileDouble operator *(VolatileDouble left, VolatileDouble right)
        {
            left.Set(left.Get() * right.Get());
            return left.Get();
        }

        public static VolatileDouble operator /(VolatileDouble left, VolatileDouble right)
        {
            left.Set(left.Get() / right.Get());
            return left.Get();
        }

        private VolatileDouble(double value)
            : this()
        {
            Set(value);
        }

        public void Set(double value)
        {
            Thread.VolatileWrite(ref this.value, value);
        }

        public double Get()
        {
            return Thread.VolatileRead(ref this.value);
        }

        public static implicit operator VolatileDouble(double value)
        {
            return new VolatileDouble(value);
        }

        public static implicit operator double(VolatileDouble value)
        {
            return value.Get();
        }

        public override string ToString()
        {
            return Get().ToString();
        }
    }
}
