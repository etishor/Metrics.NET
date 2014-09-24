using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Metrics.Utils;

namespace Metrics.Core
{
    public sealed class ExponentiallyDecayingReservoir : Reservoir, IDisposable
    {
        private const int DefaultSize = 1028;
        private const double DefaultAlpha = 0.015;
        private static readonly TimeSpan RescaleInterval = TimeSpan.FromHours(1);

        private readonly ConcurrentDictionary<double, WeightedSample> values = new ConcurrentDictionary<double, WeightedSample>();
        private readonly ReaderWriterLockSlim @lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private readonly double alpha;
        private readonly int size;
        private AtomicLong count = new AtomicLong();
        private AtomicLong startTime;

        private readonly Clock clock;

        private readonly Scheduler rescaleScheduler;

        public ExponentiallyDecayingReservoir()
            : this(DefaultSize, DefaultAlpha)
        { }

        public ExponentiallyDecayingReservoir(int size, double alpha)
            : this(size, alpha, Clock.Default, new ActionScheduler())
        { }

        public ExponentiallyDecayingReservoir(Clock clock, Scheduler scheduler)
            : this(DefaultSize, DefaultAlpha, clock, scheduler)
        { }

        public ExponentiallyDecayingReservoir(int size, double alpha, Clock clock, Scheduler scheduler)
        {
            this.size = size;
            this.alpha = alpha;
            this.clock = clock;

            this.rescaleScheduler = scheduler;
            this.rescaleScheduler.Start(RescaleInterval, () => Rescale());

            this.startTime = new AtomicLong(clock.Seconds);
        }

        public int Size { get { return Math.Min(this.size, (int)this.count.Value); } }

        public Snapshot Snapshot
        {
            get
            {
                this.@lock.EnterReadLock();
                try
                {
                    return new WeightedSnapshot(this.values.Values);
                }
                finally
                {
                    this.@lock.ExitReadLock();
                }
            }
        }

        public void Update(long value)
        {
            this.Update(value, this.clock.Seconds);
        }

        public void Reset()
        {
            this.@lock.EnterWriteLock();
            try
            {
                this.values.Clear();
                this.count.SetValue(0L);
                this.startTime = new AtomicLong(this.clock.Seconds);
            }
            finally
            {
                this.@lock.ExitWriteLock();
            }
        }

        private void Update(long value, long timestamp)
        {
            this.@lock.EnterReadLock();
            try
            {
                double itemWeight = Math.Exp(alpha * (timestamp - startTime.Value));
                var sample = new WeightedSample(value, itemWeight);
                double priority = itemWeight / ThreadLocalRandom.NextDouble();

                long newCount = count.Increment();
                if (newCount <= size)
                {
                    this.values.AddOrUpdate(priority, sample, (k, v) => sample);
                }
                else
                {
                    var first = values.First().Key;
                    if (first < priority)
                    {
                        this.values.AddOrUpdate(priority, sample, (k, v) => v);

                        WeightedSample removed;
                        // ensure we always remove an item
                        while (!values.TryRemove(first, out removed))
                        {
                            first = values.First().Key;
                        }
                    }
                }
            }
            finally
            {
                this.@lock.ExitReadLock();
            }
        }

        public void Dispose()
        {
            using (this.@lock) { }
        }

        ///* "A common feature of the above techniques—indeed, the key technique that
        // * allows us to track the decayed weights efficiently—is that they maintain
        // * counts and other quantities based on g(ti − L), and only scale by g(t − L)
        // * at query time. But while g(ti −L)/g(t−L) is guaranteed to lie between zero
        // * and one, the intermediate values of g(ti − L) could become very large. For
        // * polynomial functions, these values should not grow too large, and should be
        // * effectively represented in practice by floating point values without loss of
        // * precision. For exponential functions, these values could grow quite large as
        // * new values of (ti − L) become large, and potentially exceed the capacity of
        // * common floating point types. However, since the values stored by the
        // * algorithms are linear combinations of g values (scaled sums), they can be
        // * rescaled relative to a new landmark. That is, by the analysis of exponential
        // * decay in Section III-A, the choice of L does not affect the final result. We
        // * can therefore multiply each value based on L by a factor of exp(−α(L′ − L)),
        // * and obtain the correct value as if we had instead computed relative to a new
        // * landmark L′ (and then use this new L′ at query time). This can be done with
        // * a linear pass over whatever data structure is being used."
        // */
        private void Rescale()
        {
            this.@lock.EnterWriteLock();
            try
            {
                long oldStartTime = startTime.Value;
                this.startTime.SetValue(this.clock.Seconds);

                double scalingFactor = Math.Exp(-alpha * (startTime.Value - oldStartTime));

                var keys = new List<double>(this.values.Keys);
                foreach (var key in keys)
                {
                    WeightedSample sample;
                    if (this.values.TryRemove(key, out sample))
                    {
                        double newKey = key * Math.Exp(-alpha * (startTime.Value - oldStartTime));
                        var newSample = new WeightedSample(sample.Value, sample.Weight * scalingFactor);
                        values.AddOrUpdate(newKey, newSample, (k, v) => newSample);
                    }
                }
                // make sure the counter is in sync with the number of stored samples.
                this.count.SetValue(values.Count);
            }
            finally
            {
                this.@lock.ExitWriteLock();
            }
        }
    }
}
