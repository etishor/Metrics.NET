
using System;
using System.Threading;
namespace Metrics.Utils
{
    public static class ThreadLocalRandom
    {
        private static readonly ThreadLocal<Random> random = new ThreadLocal<Random>(() => new Random());

        public static double NextDouble()
        {
            return random.Value.NextDouble();
        }

        public static long NextLong()
        {
            byte[] buf = new byte[sizeof(long)];
            random.Value.NextBytes(buf);
            return BitConverter.ToInt64(buf, 0);
        }
    }
}
