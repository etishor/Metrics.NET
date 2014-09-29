using System.Linq;
using Metrics.Utils;

namespace Metrics
{
    /// <summary>
    /// A meter measures the rate at which a set of events occur, in a few different ways. 
    /// The mean rate is the average rate of events. It’s generally useful for trivia, 
    /// but as it represents the total rate for your application’s entire lifetime (e.g., the total number of requests handled, 
    /// divided by the number of seconds the process has been running), it doesn’t offer a sense of recency. 
    /// Luckily, meters also record three different exponentially-weighted moving average rates: the 1-, 5-, and 15-minute moving averages.
    /// </summary>
    public interface Meter : ResetableMetric, Utils.IHideObjectMembers
    {
        /// <summary>
        /// Mark the occurrence of an event.
        /// </summary>
        void Mark();

        /// <summary>
        /// Mark the occurrence of an event for an item in a set.
        /// </summary>
        void Mark(string item);

        /// <summary>
        /// Mark the occurrence of <paramref name="count"/> events.
        /// </summary>
        /// <param name="count"></param>
        void Mark(long count);

        /// <summary>
        /// Mark the occurrence of <paramref name="count"/> events for an item in a set.
        /// </summary>
        /// <param name="count"></param>
        void Mark(string item, long count);
    }

    /// <summary>
    /// The value reported by a Meter Metric
    /// </summary>
    public struct MeterValue
    {
        public struct SetItem
        {
            public readonly string Item;
            public readonly double Percent;
            public readonly MeterValue Value;

            public SetItem(string item, double percent, MeterValue value)
            {
                this.Item = item;
                this.Percent = percent;
                this.Value = value;
            }
        }

        public readonly long Count;
        public readonly double MeanRate;
        public readonly double OneMinuteRate;
        public readonly double FiveMinuteRate;
        public readonly double FifteenMinuteRate;
        public readonly SetItem[] Items;

        public MeterValue(long count, double meanRate, double oneMinuteRate, double fiveMinuteRate, double fifteenMinuteRate, SetItem[] items)
        {
            this.Count = count;
            this.MeanRate = meanRate;
            this.OneMinuteRate = oneMinuteRate;
            this.FiveMinuteRate = fiveMinuteRate;
            this.FifteenMinuteRate = fifteenMinuteRate;
            this.Items = items;
        }

        public MeterValue Scale(TimeUnit unit)
        {
            var factor = unit.ToSeconds(1);
            return new MeterValue(this.Count,
                this.MeanRate * factor,
                this.OneMinuteRate * factor,
                this.FiveMinuteRate * factor,
                this.FifteenMinuteRate * factor,
                this.Items.Select(i => new SetItem(i.Item, i.Percent, i.Value.Scale(unit))).ToArray());
        }
    }

    /// <summary>
    /// Combines the value of the meter with the defined unit and the rate unit at which the value is reported.
    /// </summary>
    public sealed class MeterValueSource : MetricValueSource<MeterValue>
    {
        public MeterValueSource(string name, MetricValueProvider<MeterValue> value, Unit unit, TimeUnit rateUnit, MetricTags tags)
            : base(name, value, unit, tags)
        {
            this.RateUnit = rateUnit;
        }

        public TimeUnit RateUnit { get; private set; }
    }
}
