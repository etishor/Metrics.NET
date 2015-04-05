
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using ConcurrencyUtilities;
using Metrics.MetricData;
using Metrics.Utils;
namespace Metrics.Core
{
    public interface MeterImplementation : Meter, MetricValueProvider<MeterValue> { }

    public sealed class MeterMetric : MeterImplementation, IDisposable
    {
        private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(5);

        private struct MeterWrapper
        {
            private readonly ThreadLocalLongAdder counter;
            private readonly EWMA m1Rate;
            private readonly EWMA m5Rate;
            private readonly EWMA m15Rate;

            public MeterWrapper(ThreadLocalLongAdder counter)
            {
                this.counter = counter;
                this.m1Rate = EWMA.OneMinuteEWMA();
                this.m5Rate = EWMA.FiveMinuteEWMA();
                this.m15Rate = EWMA.FifteenMinuteEWMA();
            }

            public void Tick()
            {
                this.m1Rate.Tick();
                this.m5Rate.Tick();
                this.m15Rate.Tick();
            }

            public void Mark(long count)
            {
                this.counter.Add(count);
                this.m1Rate.Update(count);
                this.m5Rate.Update(count);
                this.m15Rate.Update(count);
            }

            public void Merge(MeterWrapper other)
            {
                this.counter.Add(other.counter.GetValue());
                this.m1Rate.Merge(other.m1Rate);
                this.m5Rate.Merge(other.m5Rate);
                this.m15Rate.Merge(other.m15Rate);
            }

            public void Reset()
            {
                this.counter.Reset();
                this.m1Rate.Reset();
                this.m5Rate.Reset();
                this.m15Rate.Reset();
            }

            public MeterValue GetValue(double elapsed)
            {
                var count = this.counter.GetValue();
                return new MeterValue(count, GetMeanRate(count, elapsed), OneMinuteRate, FiveMinuteRate, FifteenMinuteRate, TimeUnit.Seconds);
            }

            private static double GetMeanRate(long value, double elapsed)
            {
                if (value == 0)
                {
                    return 0.0;
                }

                return value / elapsed * TimeUnit.Seconds.ToNanoseconds(1);
            }

            private double FifteenMinuteRate { get { return this.m15Rate.GetRate(TimeUnit.Seconds); } }
            private double FiveMinuteRate { get { return this.m5Rate.GetRate(TimeUnit.Seconds); } }
            private double OneMinuteRate { get { return this.m1Rate.GetRate(TimeUnit.Seconds); } }
        }

        private ConcurrentDictionary<string, MeterWrapper> setMeters = null;

        private readonly MeterWrapper wrapper = new MeterWrapper(new ThreadLocalLongAdder());

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
            this.tickScheduler.Start(TickInterval, (Action)Tick);
        }

        public MeterValue Value { get { return GetValue(); } }

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
            Mark(item, 1L);
        }

        public void Mark(string item, long count)
        {
            Mark(count);

            if (item == null)
            {
                return;
            }

            if (this.setMeters == null)
            {
                Interlocked.CompareExchange(ref this.setMeters, new ConcurrentDictionary<string, MeterWrapper>(), null);
            }

            Debug.Assert(this.setMeters != null);
            this.setMeters.GetOrAdd(item, v => new MeterWrapper(new ThreadLocalLongAdder())).Mark(count);
        }

        public MeterValue GetValue(bool resetMetric = false)
        {
            if (this.setMeters == null || this.setMeters.Count == 0)
            {
                double elapsed = (this.clock.Nanoseconds - this.startTime);
                var value = this.wrapper.GetValue(elapsed);
                if (resetMetric)
                {
                    Reset();
                }
                return value;
            }

            return GetValueWithSetItems(resetMetric);
        }

        private MeterValue GetValueWithSetItems(bool resetMetric)
        {
            double elapsed = this.clock.Nanoseconds - this.startTime;
            var value = this.wrapper.GetValue(elapsed);

            Debug.Assert(this.setMeters != null);

            var items = new MeterValue.SetItem[this.setMeters.Count];
            var index = 0;

            foreach (var meter in this.setMeters)
            {
                var itemValue = meter.Value.GetValue(elapsed);
                var percent = value.Count > 0 ? itemValue.Count / (double)value.Count * 100 : 0.0;
                items[index++] = new MeterValue.SetItem(meter.Key, percent, itemValue);
                if (index == items.Length)
                {
                    break;
                }
            }

            Array.Sort(items, MeterValue.SetItemComparer);
            var result = new MeterValue(value.Count, value.MeanRate, value.OneMinuteRate, value.FiveMinuteRate, value.FifteenMinuteRate, TimeUnit.Seconds, items);
            if (resetMetric)
            {
                Reset();
            }
            return result;
        }

        private void Tick()
        {
            this.wrapper.Tick();
            if (this.setMeters != null)
            {
                foreach (var value in this.setMeters.Values)
                {
                    value.Tick();
                }
            }
        }

        public void Dispose()
        {
            this.tickScheduler.Stop();
            using (this.tickScheduler) { }

            if (this.setMeters != null)
            {
                this.setMeters.Clear();
                this.setMeters = null;
            }
        }

        public void Reset()
        {
            this.startTime = this.clock.Nanoseconds;
            this.wrapper.Reset();
            if (this.setMeters != null)
            {
                foreach (var meter in this.setMeters.Values)
                {
                    meter.Reset();
                }
            }
        }

        public bool Merge(MetricValueProvider<MeterValue> other)
        {
            var mOther = other as MeterMetric;
            if (mOther == null)
            {
                return false;
            }

            this.wrapper.Merge(mOther.wrapper);
            if (this.setMeters != null && mOther.setMeters != null)
            {
                foreach (var key in mOther.setMeters)
                {
                    this.setMeters.GetOrAdd(key.Key, v => new MeterWrapper(new ThreadLocalLongAdder())).Merge(key.Value);
                }
            }

            return true;
        }
    }
}
