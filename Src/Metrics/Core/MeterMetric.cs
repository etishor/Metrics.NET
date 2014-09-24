
using System;
using Metrics.Utils;
namespace Metrics.Core
{
    public interface MeterImplementation : Meter, MetricValueProvider<MeterValue> { }

    public sealed class MeterMetric : MeterImplementation, IDisposable
    {
        public static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(5);

        private readonly EWMA m1Rate = EWMA.OneMinuteEWMA();
        private readonly EWMA m5Rate = EWMA.FiveMinuteEWMA();
        private readonly EWMA m15Rate = EWMA.FifteenMinuteEWMA();

        private readonly Clock clock;
        private long startTime;

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
            this.tickScheduler.Start(TickInterval, () => Tick());
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
                return this.m15Rate.GetRate(TimeUnit.Seconds);
            }
        }

        private double FiveMinuteRate
        {
            get
            {
                return this.m5Rate.GetRate(TimeUnit.Seconds);
            }
        }

        private double OneMinuteRate
        {
            get
            {
                return this.m1Rate.GetRate(TimeUnit.Seconds);
            }
        }

        private void Tick()
        {
            this.m1Rate.Tick();
            this.m5Rate.Tick();
            this.m15Rate.Tick();
        }

        public void Dispose()
        {
            this.tickScheduler.Stop();
            using (this.tickScheduler) { }
        }

        public void Reset()
        {
            this.count.SetValue(0);
            this.m1Rate.Reset();
            this.m5Rate.Reset();
            this.m15Rate.Reset();
            this.startTime = this.clock.Nanoseconds;
        }
    }
}
