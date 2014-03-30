
using Metrics.Utils;
namespace Metrics.Core
{
    public class MeterMetric : Meter
    {
        private static readonly long TickInterval = TimeUnit.Seconds.ToNanoseconds(5);

        private readonly EWMA m1Rate = EWMA.OneMinuteEWMA();
        private readonly EWMA m5Rate = EWMA.FiveMinuteEWMA();
        private readonly EWMA m15Rate = EWMA.FifteenMinuteEWMA();

        private readonly Clock clock;
        private readonly long startTime;

        private readonly AtomicLong count = new AtomicLong();
        private readonly AtomicLong lastTick = new AtomicLong();

        public MeterMetric() : this(Clock.System) { }

        public MeterMetric(Clock clock)
        {
            this.clock = clock;
            this.startTime = this.clock.Nanoseconds;
            this.lastTick.Value = startTime;
        }

        public void Mark()
        {
            Mark(1L);
        }

        public void Mark(long count)
        {
            TickIfNecessary();
            this.count.Add(count);
            this.m1Rate.Update(count);
            this.m5Rate.Update(count);
            this.m15Rate.Update(count);
        }

        public MeterValue Value
        {
            get
            {
                return new MeterValue(this.count.Value, this.MeanRate, this.OneMinuteRate, this.FiveMinuteRate, this.FifteenMinuteRate);
            }
        }

        private double MeanRate
        {
            get
            {
                if (this.count.Value == 0)
                {
                    return 0.0;
                }

                double elapsed = (clock.Nanoseconds - startTime);
                return this.count.Value / elapsed * TimeUnit.Seconds.ToNanoseconds(1);
            }
        }

        private double FifteenMinuteRate
        {
            get
            {
                TickIfNecessary();
                return m15Rate.GetRate(TimeUnit.Seconds);
            }
        }

        private double FiveMinuteRate
        {
            get
            {
                TickIfNecessary();
                return m5Rate.GetRate(TimeUnit.Seconds);
            }
        }

        private double OneMinuteRate
        {
            get
            {
                TickIfNecessary();
                return m1Rate.GetRate(TimeUnit.Seconds);
            }
        }

        private void TickIfNecessary()
        {
            long oldTick = lastTick.Value;
            long newTick = clock.Nanoseconds;
            long age = newTick - oldTick;

            if (age > TickInterval)
            {
                long newIntervalStartTick = newTick - age % TickInterval;
                if (lastTick.CompareAndSet(oldTick, newIntervalStartTick))
                {
                    long requiredTicks = age / TickInterval;
                    for (long i = 0; i < requiredTicks; i++)
                    {
                        m1Rate.Tick();
                        m5Rate.Tick();
                        m15Rate.Tick();
                    }
                }
            }
        }

    }
}
