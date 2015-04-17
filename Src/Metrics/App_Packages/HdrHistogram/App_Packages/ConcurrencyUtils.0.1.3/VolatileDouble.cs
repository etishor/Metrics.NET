// This is a collection of .NET concurrency utilities, inspired by the classes
// available in java. This utilities are written by Iulian Margarintescu as described here
// https://github.com/etishor/ConcurrencyUtilities
// 
//
// Striped64 & LongAdder classes were ported from Java and had this copyright:
// 
// Written by Doug Lea with assistance from members of JCP JSR-166
// Expert Group and released to the public domain, as explained at
// http://creativecommons.org/publicdomain/zero/1.0/
// 
// Source: http://gee.cs.oswego.edu/cgi-bin/viewcvs.cgi/jsr166/src/jsr166e/Striped64.java?revision=1.8

//
// By default all added classes are internal to your assembly. 
// To make them public define you have to define the conditional compilation symbol CONCURRENCY_UTILS_PUBLIC in your project properties.
//

#pragma warning disable 1591

// ReSharper disable All

using System.Threading;

namespace HdrHistogram.ConcurrencyUtilities
{
    /// <summary>
    /// Double value on which the GetValue/SetValue operations are performed using Volatile.Read/Volatile.Write.
    /// </summary>
    /// <remarks>
    /// This datastructure is a struct. If a member is declared readonly VolatileDouble calling set will *NOT* modify the value.
    /// GetValue/SetValue are expressed as methods to make it obvious that a non-trivial operation is performed.
    /// </remarks>
#if CONCURRENCY_UTILS_PUBLIC
public
#else
internal
#endif
    struct VolatileDouble
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
