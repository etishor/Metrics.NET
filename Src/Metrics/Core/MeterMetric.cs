
using System;
using System.Collections.Concurrent;
using System.Linq;
using Metrics.MetricData;
using Metrics.Utils;
namespace Metrics.Core
{
    public interface MeterImplementation : Meter, MetricValueProvider<MeterValue> { }

    public sealed class MeterMetric : MeterImplementation, IDisposable
    {
        public static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(5);

        private class MeterWrapper
        {
            public readonly EWMA m1Rate = EWMA.OneMinuteEWMA();
            public readonly EWMA m5Rate = EWMA.FiveMinuteEWMA();
            public readonly EWMA m15Rate = EWMA.FifteenMinuteEWMA();
            public AtomicLong count = new AtomicLong();

            public void Tick()
            {
                this.m1Rate.Tick();
                this.m5Rate.Tick();
                this.m15Rate.Tick();
            }

            public void Mark(long count)
            {
                this.count.Add(count);
                this.m1Rate.Update(count);
                this.m5Rate.Update(count);
                this.m15Rate.Update(count);
            }

            public void Reset()
            {
                this.count.SetValue(0);
                this.m1Rate.Reset();
                this.m5Rate.Reset();
                this.m15Rate.Reset();
            }

            public MeterValue GetValue(double elapsed)
            {
                return new MeterValue(this.count.Value, this.GetMeanRate(elapsed), this.OneMinuteRate, this.FiveMinuteRate, this.FifteenMinuteRate, TimeUnit.Seconds);
            }

            private double GetMeanRate(double elapsed)
            {
                if (this.count.Value == 0)
                {
                    return 0.0;
                }

                return this.count.Value / elapsed * TimeUnit.Seconds.ToNanoseconds(1);
            }

            private double FifteenMinuteRate { get { return this.m15Rate.GetRate(TimeUnit.Seconds); } }
            private double FiveMinuteRate { get { return this.m5Rate.GetRate(TimeUnit.Seconds); } }
            private double OneMinuteRate { get { return this.m1Rate.GetRate(TimeUnit.Seconds); } }
        }


        private readonly ConcurrentDictionary<string, MeterWrapper> setMeters = new ConcurrentDictionary<string, MeterWrapper>();

        private readonly MeterWrapper wrapper = new MeterWrapper();

        private readonly Clock clock;
        private readonly Scheduler tickScheduler;

        private long startTime;

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
            this.wrapper.Mark(count);
        }

        public void Mark(string item)
        {
            this.Mark(item, 1L);
        }

        public void Mark(string item, long count)
        {
            this.Mark(count);
            this.setMeters.GetOrAdd(item, v => new MeterWrapper()).Mark(count);
        }

        public MeterValue GetValue(bool resetMetric = false)
        {
            var value = this.Value;
            if (resetMetric)
            {
                this.Reset();
            }
            return value;
        }

        public MeterValue Value
        {
            get
            {
                double elapsed = (clock.Nanoseconds - startTime);
                var value = this.wrapper.GetValue(elapsed);

                var items = this.setMeters
                    .Select(m => new { Item = m.Key, Value = m.Value.GetValue(elapsed) })
                    .Select(m => new MeterValue.SetItem(m.Item, value.Count > 0 ? m.Value.Count / (double)value.Count * 100 : 0.0, m.Value))
                    .OrderBy(m => m.Percent)
                    .ThenBy(m => m.Item)
                    .ToArray();

                return new MeterValue(value.Count, value.MeanRate, value.OneMinuteRate, value.FiveMinuteRate, value.FifteenMinuteRate, TimeUnit.Seconds, items);
            }
        }

        private void Tick()
        {
            this.wrapper.Tick();
            foreach (var value in setMeters.Values)
            {
                value.Tick();
            }
        }

        public void Dispose()
        {
            this.tickScheduler.Stop();
            using (this.tickScheduler) { }
            this.setMeters.Clear();
        }

        public void Reset()
        {
            this.startTime = this.clock.Nanoseconds;
            this.wrapper.Reset();
            foreach (var meter in this.setMeters.Values)
            {
                meter.Reset();
            }
        }
    }
}
