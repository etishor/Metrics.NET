
namespace Metrics
{
    /// <summary>
    /// A counter is a simple incrementing and decrementing 64-bit integer.
    /// </summary>
    public interface Counter
    {
        void Increment();
        void Increment(long value);

        void Decrement();
        void Decrement(long value);
    }
}
