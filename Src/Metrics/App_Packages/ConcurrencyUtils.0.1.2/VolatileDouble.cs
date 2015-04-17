using System.Threading;

namespace Metrics
{
    /// <summary>
    /// Double value on which the GetValue/SetValue operations are performed using Volatile.Read/Volatile.Write.
    /// </summary>
    /// <remarks>
    /// This datastructure is a struct. If a member is declared readonly VolatileDouble calling set will *NOT* modify the value.
    /// GetValue/SetValue are expressed as methods to make it obvious that a non-trivial operation is performed.
    /// </remarks>
    public struct VolatileDouble
#if INTERNAL_INTERFACES
 : VolatileValue<double>
#endif
    {
        private double value;

        /// <summary>
        /// Initialize the value of this instance
        /// </summary>
        /// <param name="value">Initial value of the instance.</param>
        public VolatileDouble(double value)
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
        public void SetValue(double newValue)
        {
            Volatile.Write(ref this.value, newValue);
        }

        /// <summary>
        /// Get the current value of this instance
        /// </summary>
        /// <returns>The current value of the instance</returns>
        public double GetValue()
        {
            return Volatile.Read(ref this.value);
        }
    }
}
