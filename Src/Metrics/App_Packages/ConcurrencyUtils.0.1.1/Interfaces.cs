
namespace Metrics
{
#if INTERNAL_INTERFACES
    /*
     * This interfaces are only used to maintain a consistent signature of the different implementations
     *  
     */


    internal interface ValueReader<out T>
    {
        T GetValue();
    }

    internal interface ValueWriter<in T>
    {
        void SetValue(T newValue);
    }

    internal interface VolatileValue<T> : ValueReader<T>, ValueWriter<T> { }

    internal interface ValueAdder<T> : ValueReader<T>
    {
        T GetAndReset();
        void Add(T value);
        void Increment();
        void Increment(T value);
        void Decrement();
        void Decrement(T value);
        void Reset();
    }

    internal interface AtomicValue<T> : VolatileValue<T>
    {
        T Add(T value);

        T GetAndAdd(T value);
        T GetAndIncrement();
        T GetAndIncrement(T value);
        T GetAndDecrement();
        T GetAndDecrement(T value);

        T Increment();
        T Increment(T value);
        T Decrement();
        T Decrement(T value);

        T GetAndReset();
        T GetAndSet(T newValue);
        bool CompareAndSwap(T expected, T updated);
    }
#endif
}
