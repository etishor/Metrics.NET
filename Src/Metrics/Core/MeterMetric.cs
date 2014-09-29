
using System;
using System.Collections.Concurrent;
using System.Linq;
using Metrics.Utils;
namespace Metrics.Core
{
    public interface MeterImplementation : Meter, MetricValueProvider<MeterValue> { }

    public sealed class MeterMetric : MeterImplementation, IDisposable
    {
        public static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(5);

        private readonly ConcurrentDictionary<string, MeterMetric> setMeters = new ConcurrentDictionary<string, MeterMetric>();

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

        public void Mark(string item)
        {
            this.Mark(item, 1L);
        }

        public void Mark(string item, long count)
        {
            this.Mark(count);
            this.setMeters.GetOrAdd(item, v => new MeterMetric(this.clock, this.tickScheduler)).Mark(count);
        }

        public MeterValue Value
        {
            get
            {
                var count = this.count.Value;
                var items = this.setMeters
                    .Select(m => new { Item = m.Key, Value = m.Value.Value })
                    .Select(m => new MeterValue.SetItem(m.Item, m.Value.Count / (double)count * 100, m.Value))
                    .OrderBy(m => m.Item)
                    .ToArray();

                return new MeterValue(this.count.Value, this.MeanRate, this.OneMinuteRate, this.FiveMinuteRate, this.FifteenMinuteRate, items);
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

            foreach (var key in this.setMeters.Keys)
            {
                MeterMetric metric;
                if (this.setMeters.TryRemove(key, out metric))
                {
                    using (metric) { }
                }
            }
        }

        public void Reset()
        {
            this.count.SetValue(0);
            this.m1Rate.Reset();
            this.m5Rate.Reset();
            this.m15Rate.Reset();
            this.startTime = this.clock.Nanoseconds;
            foreach (var meter in this.setMeters.Values)
            {
                meter.Reset();
            }
        }
    }
}
