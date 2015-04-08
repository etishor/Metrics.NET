using System;
using System.Threading;

namespace Metrics
{
    /// <summary>
    /// Helper class to generate Random values is a thread safe way. Not suitable for cryptographic operations.
    /// </summary>
    public static class ThreadLocalRandom
    {
        private static readonly ThreadLocal<Random> LocalRandom = new ThreadLocal<Random>(() => new Random(Thread.CurrentThread.ManagedThreadId));

        public static int Next() { return LocalRandom.Value.Next(); }
        public static int Next(int maxValue) { return LocalRandom.Value.Next(); }
        public static int Next(int minValue, int maxValue) { return LocalRandom.Value.Next(); }
        public static void NextBytes(byte[] buffer) { LocalRandom.Value.NextBytes(buffer); }
        public static double NextDouble() { return LocalRandom.Value.NextDouble(); }

        public static long NextLong()
        {
            long heavy = LocalRandom.Value.Next();
            long light = LocalRandom.Value.Next();
            return heavy << 32 | light;
        }

        public static long NextLong(long max)
        {
            const int BitsPerLong = 63;
            long bits, val;
            do
            {
                bits = NextLong() & (~(1L << BitsPerLong));
                val = bits % max;
            } while (bits - val + (max - 1) < 0L);
            return val;
        }
    }
}
