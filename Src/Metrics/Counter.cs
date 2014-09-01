
namespace Metrics
{
    /// <summary>
    /// A counter is a simple incrementing and decrementing 64-bit integer.
    /// </summary>
    public interface Counter : Utils.IHideObjectMembers
    {
        void Increment();
        void Increment(long value);

        void Decrement();
        void Decrement(long value);
    }

    /// <summary>
    /// Combines the value for a counter with the defined unit for the value.
    /// </summary>
    public sealed class CounterValueSource : MetricValueSource<long>
    {
        public CounterValueSource(string name, MetricValueProvider<long> value, Unit unit)
            : base(name, value, unit)
        { }
    }
}
