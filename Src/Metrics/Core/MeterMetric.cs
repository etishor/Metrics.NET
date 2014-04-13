
using System;
using Metrics.Utils;
namespace Metrics.Core
{
    public sealed class MeterMetric : Meter, MetricValueProvider<MeterValue>, IDisposable
    {
        public static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(5);

        private readonly EWMA m1Rate = EWMA.OneMinuteEWMA();
        private readonly EWMA m5Rate = EWMA.FiveMinuteEWMA();
        private readonly EWMA m15Rate = EWMA.FifteenMinuteEWMA();

        private readonly Clock clock;
        private readonly long startTime;

        private AtomicLong count = new AtomicLong();
        private readonly Scheduler tickScheduler;

        public MeterMetric()
            : this(Clock.Default, new ActionScheduler())
        { }

        public MeterMetric(Clock clock, Scheduler scheduler)
        {
            this.clock = clock;
            this.startTime = this.clock.Nanoseconds;
            this.tickScheduler = scheduler;
            this.tickScheduler.Start(TickInterval, Tick);
        }

        public void Mark()
        {
            Mark(1L);
        }

        public void Mark(long count)
        {
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
                return m15Rate.GetRate(TimeUnit.Seconds);
            }
        }

        private double FiveMinuteRate
        {
            get
            {
                return m5Rate.GetRate(TimeUnit.Seconds);
            }
        }

        private double OneMinuteRate
        {
            get
            {
                return m1Rate.GetRate(TimeUnit.Seconds);
            }
        }

        private void Tick()
        {
            m1Rate.Tick();
            m5Rate.Tick();
            m15Rate.Tick();
        }

        public void Dispose()
        {
            this.tickScheduler.Stop();
            using (this.tickScheduler) { }
        }
    }
}
