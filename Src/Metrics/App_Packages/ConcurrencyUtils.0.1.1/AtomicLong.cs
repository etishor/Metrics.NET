using System.Threading;

namespace Metrics
{
    /// <summary>
    /// Atomic long value. Operations exposed on this class are performed using System.Threading.Interlocked class and are thread safe.
    /// For AtomicLong values that are stored in arrays PaddedAtomicLong is recommended.
    /// </summary>
    /// <remarks>
    /// The AtomicLong is a struct not a class and members of this type should *not* be declared readonly or changes will not be reflected in the member instance. 
    /// </remarks>
    public struct AtomicLong
#if INTERNAL_INTERFACES
        : AtomicValue<long>, ValueAdder<long>
#endif
    {
        private long value;

        /// <summary>
        /// Initializes a new instance with the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Initial value of the instance.</param>
        public AtomicLong(long value)
        {
            this.value = value;
        }

        /// <summary>
        /// Returns the latest value of this instance written by any processor.
        /// </summary>
        /// <returns>The latest written value of this instance.</returns>
        public long GetValue()
        {
            return Thread.VolatileRead(ref this.value);
        }

        /// <summary>
        /// Write a new value to this instance. The value is immediately seen by all processors.
        /// </summary>
        /// <param name="value">The new value for this instance.</param>
        public void SetValue(long value)
        {
            Thread.VolatileWrite(ref this.value, value);
        }

        /// <summary>
        /// Add <paramref name="value"/> to this instance and return the resulting value.
        /// </summary>
        /// <param name="value">The amount to add.</param>
        /// <returns>The value of this instance + the amount added.</returns>
        public long Add(long value)
        {
            return Interlocked.Add(ref this.value, value);
        }

        /// <summary>
        /// Add <paramref name="value"/> to this instance and return the value this instance had before the add operation.
        /// </summary>
        /// <param name="value">The amount to add.</param>
        /// <returns>The value of this instance before the amount was added.</returns>
        public long GetAndAdd(long value)
        {
            return Add(value) - value;
        }

        /// <summary>
        /// Increment this instance and return the value the instance had before the increment.
        /// </summary>
        /// <returns>The value of the instance *before* the increment.</returns>
        public long GetAndIncrement()
        {
            return Increment() - 1;
        }

        /// <summary>
        /// Increment this instance with <paramref name="value"/> and return the value the instance had before the increment.
        /// </summary>
        /// <returns>The value of the instance *before* the increment.</returns>
        public long GetAndIncrement(long value)
        {
            return Increment(value) - value;
        }

        /// <summary>
        /// Decrement this instance and return the value the instance had before the decrement.
        /// </summary>
        /// <returns>The value of the instance *before* the decrement.</returns>
        public long GetAndDecrement()
        {
            return Decrement() + 1;
        }

        /// <summary>
        /// Decrement this instance with <paramref name="value"/> and return the value the instance had before the decrement.
        /// </summary>
        /// <returns>The value of the instance *before* the decrement.</returns>
        public long GetAndDecrement(long value)
        {
            return Decrement(value) + value;
        }

        /// <summary>
        /// Increment this instance and return the value after the increment.
        /// </summary>
        /// <returns>The value of the instance *after* the increment.</returns>
        public long Increment()
        {
            return Interlocked.Increment(ref this.value);
        }

        /// <summary>
        /// Increment this instance with <paramref name="value"/> and return the value after the increment.
        /// </summary>
        /// <returns>The value of the instance *after* the increment.</returns>
        public long Increment(long value)
        {
            return Add(value);
        }

        /// <summary>
        /// Decrement this instance and return the value after the decrement.
        /// </summary>
        /// <returns>The value of the instance *after* the decrement.</returns>
        public long Decrement()
        {
            return Interlocked.Decrement(ref this.value);
        }

        /// <summary>
        /// Decrement this instance with <paramref name="value"/> and return the value after the decrement.
        /// </summary>
        /// <returns>The value of the instance *after* the decrement.</returns>
        public long Decrement(long value)
        {
            return Add(-value);
        }

        /// <summary>
        /// Returns the current value of the instance and sets it to zero as an atomic operation.
        /// </summary>
        /// <returns>The current value of the instance.</returns>
        public long GetAndReset()
        {
            return GetAndSet(0L);
        }

        /// <summary>
        /// Returns the current value of the instance and sets it to <paramref name="newValue"/> as an atomic operation.
        /// </summary>
        /// <returns>The current value of the instance.</returns>
        public long GetAndSet(long newValue)
        {
            return Interlocked.Exchange(ref this.value, newValue);
        }

        /// <summary>
        /// Replace the value of this instance, if the current value is equal to the <paramref name="expected"/> value.
        /// </summary>
        /// <param name="expected">Value this instance is expected to be equal with.</param>
        /// <param name="updated">Value to set this instance to, if the current value is equal to the expected value</param>
        /// <returns>True if the update was made, false otherwise.</returns>
        public bool CompareAndSwap(long expected, long updated)
        {
            return Interlocked.CompareExchange(ref this.value, updated, expected) == expected;
        }

#if INTERNAL_INTERFACES
        long ValueAdder<long>.GetAndReset() { return this.GetAndReset(); }
        void ValueAdder<long>.Add(long value) { this.Add(value); }
        void ValueAdder<long>.Increment() { this.Increment(); }
        void ValueAdder<long>.Increment(long value) { this.Increment(value); }
        void ValueAdder<long>.Decrement() { this.Decrement(); }
        void ValueAdder<long>.Decrement(long value) { this.Decrement(value); }
        void ValueAdder<long>.Reset() { this.SetValue(0L); }
        long ValueReader<long>.GetValue() { return this.GetValue(); }
#endif
    }
}
