
using System;
namespace Metrics.MetricData
{

    public struct CounterValue
    {
        public struct SetItem
        {
            /// <summary>
            /// Registered item name.
            /// </summary>
            public readonly string Item;

            /// <summary>
            /// Specific count for this item.
            /// </summary>
            public readonly long Count;

            /// <summary>
            /// Percent of this item from the total count.
            /// </summary>
            public readonly double Percent;

            public SetItem(string item, long count, double percent)
            {
                this.Item = item;
                this.Count = count;
                this.Percent = percent;
            }
        }

        /// <summary>
        /// Total count of the counter instance.
        /// </summary>
        public readonly long Count;

        /// <summary>
        /// Separate counters for each registered set item.
        /// </summary>
        public readonly SetItem[] Items;

        public CounterValue(long count, SetItem[] items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            this.Count = count;
            this.Items = items;
        }
    }

    /// <summary>
    /// Combines the value for a counter with the defined unit for the value.
    /// </summary>
    public sealed class CounterValueSource : MetricValueSource<CounterValue>
    {
        public CounterValueSource(string name, MetricValueProvider<CounterValue> value, Unit unit, MetricTags tags)
            : base(name, value, unit, tags)
        { }
    }
}
