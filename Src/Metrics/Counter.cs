
namespace Metrics
{
    /// <summary>
    /// A counter is a simple incrementing and decrementing 64-bit integer.
    /// </summary>
    public interface Counter : ResetableMetric, Utils.IHideObjectMembers
    {
        /// <summary>
        /// Increment the counter value.
        /// </summary>
        void Increment();

        /// <summary>
        /// Increment the counter value for an item from a set.
        /// The counter will record the count for each item and percentage from total count.
        /// </summary>
        /// <param name="item">Item from the set for which to increment the counter value.</param>
        void Increment(string item);

        /// <summary>
        /// Increment the counter value with a specified amount.
        /// </summary>
        /// <param name="amount">The amount with which to increment the counter.</param>
        void Increment(long amount);

        /// <summary>
        /// Increment the counter value with a specified amount for an item from a set.
        /// The counter will record the count for each item and percentage from total count.
        /// </summary>
        /// <param name="item">Item from the set for which to increment the counter value.</param>
        /// <param name="amount">The amount with which to increment the counter.</param>
        void Increment(string item, long amount);

        /// <summary>
        /// Decrement the counter value.
        /// </summary>
        void Decrement();

        /// <summary>
        /// Decrement the counter value for an item from a set.
        /// </summary>
        /// <param name="item">Item from the set for which to increment the counter value.</param>
        void Decrement(string item);

        /// <summary>
        /// Decrement the counter value with a specified amount.
        /// </summary>
        /// <param name="amount">The amount with which to increment the counter.</param>
        void Decrement(long amount);

        /// <summary>
        /// Decrement the counter value with a specified amount for an item from a set.
        /// </summary>
        /// <param name="item">Item from the set for which to increment the counter value.</param>
        /// <param name="amount">The amount with which to increment the counter.</param>
        void Decrement(string item, long amount);
    }

    public struct CounterValue
    {
        public struct SetItem
        {
            public readonly string Item;
            public readonly long Count;
            public readonly double Percent;

            public SetItem(string item, long count, double percent)
            {
                this.Item = item;
                this.Count = count;
                this.Percent = percent;
            }
        }

        public readonly long Count;
        public readonly SetItem[] Items;

        public CounterValue(long count, SetItem[] items)
        {
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
