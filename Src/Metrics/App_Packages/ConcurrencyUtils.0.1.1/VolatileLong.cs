using System.Threading;

namespace Metrics
{
    /// <summary>
    /// Long value on which the GetValue/SetValue operations are performed using Thread.VolatileWrite/Thread.VolatileWrite.
    /// </summary>
    /// <remarks>
    /// This datastructure is a struct. If a member is declared readonly VolatileLong calling set will *NOT* modify the value.
    /// GetValue/SetValue are expressed as methods to make it obvious that a non-trivial operation is performed.
    /// </remarks>
    public struct VolatileLong
#if INTERNAL_INTERFACES
        : VolatileValue<long>
#endif
    {
        private long value;

        /// <summary>
        /// Initialize the value of this instance
        /// </summary>
        /// <param name="value">Initial value of the instance.</param>
        public VolatileLong(long value)
        {
            this.value = value;
        }

        /// <summary>
        /// Set the the value of this instance to <paramref name="newValue"/>
        /// </summary>
        /// <remarks>
        /// Don't call Set on readonly fields.
        /// </remarks>
        /// <param name="newValue">New value for this instance</param>
        public void SetValue(long newValue)
        {
            Thread.VolatileWrite(ref this.value, newValue);
        }

        /// <summary>
        /// Get the current value of this instance
        /// </summary>
        /// <returns>The current value of the instance</returns>
        public long GetValue()
        {
            return Thread.VolatileRead(ref this.value);
        }
    }
}
