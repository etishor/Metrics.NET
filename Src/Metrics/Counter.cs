
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
        /// Increment the counter value with a specified amount.
        /// </summary>
        /// <param name="value">The amount with which to increment the counter.</param>
        void Increment(long value);

        /// <summary>
        /// Decrement the counter value.
        /// </summary>
        void Decrement();

        /// <summary>
        /// Decrement the counter value with a specified amount.
        /// </summary>
        /// <param name="value">The amount with which to increment the counter.</param>
        void Decrement(long value);
    }

    /// <summary>
    /// Combines the value for a counter with the defined unit for the value.
    /// </summary>
    public sealed class CounterValueSource : MetricValueSource<long>
    {
        public CounterValueSource(string name, MetricValueProvider<long> value, Unit unit, MetricTags tags)
            : base(name, value, unit, tags)
        { }
    }
}
