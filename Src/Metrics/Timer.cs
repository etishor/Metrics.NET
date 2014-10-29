using System;

namespace Metrics
{
    /// <summary>
    /// A timer is basically a histogram of the duration of a type of event and a meter of the rate of its occurrence.
    /// <seealso cref="Histogram"/> and <seealso cref="Meter"/>
    /// </summary>
    public interface Timer : ResetableMetric, Utils.IHideObjectMembers
    {
        /// <summary>
        /// Manually record timer value
        /// </summary>
        /// <param name="time">The value representing the manually measured time.</param>
        /// <param name="unit">Unit for the value.</param>
        /// <param name="userValue">A custom user value that will be associated to the results.
        /// Useful for tracking (for example) for which id the max or min value was recorded.
        /// </param>

        void Record(long time, TimeUnit unit, string userValue = null);

        /// <summary>
        /// Runs the <paramref name="action"/> and records the time it took.
        /// </summary>
        /// <param name="action">Action to run and record time for.</param>
        /// <param name="userValue">A custom user value that will be associated to the results.
        /// Useful for tracking (for example) for which id the max or min value was recorded.
        /// </param>
        void Time(Action action, string userValue = null);

        /// <summary>
        /// Runs the <paramref name="action"/> returning the result and records the time it took.
        /// </summary>
        /// <typeparam name="T">Type of the value returned by the action</typeparam>
        /// <param name="action">Action to run and record time for.</param>
        /// <param name="userValue">A custom user value that will be associated to the results.
        /// Useful for tracking (for example) for which id the max or min value was recorded.
        /// </param>
        /// <returns>The result of the <paramref name="action"/></returns>
        T Time<T>(Func<T> action, string userValue = null);

        /// <summary>
        /// Creates a new disposable instance and records the time it takes until the instance is disposed.
        /// <code>
        /// using(timer.NewContext())
        /// {
        ///     ExecuteMethodThatNeedsMonitoring();
        /// }
        /// </code>
        /// </summary>
        /// <param name="userValue">A custom user value that will be associated to the results.
        /// Useful for tracking (for example) for which id the max or min value was recorded.
        /// </param>
        /// <returns>A disposable instance that will record the time passed until disposed.</returns>
        TimerContext NewContext(string userValue = null);

        /// <summary>
        /// Creates a new disposable instance and records the time it takes until the instance is disposed.
        /// The <paramref name="finalAction"/> action is called after the context has been disposed
        /// <code>
        /// using(timer.NewContext( t => log.Debug(t)))
        /// {
        ///     ExecuteMethodThatNeedsMonitoring();
        /// }
        /// </code>
        /// </summary>
        /// <param name="finalAction">Action to call after the context is disposed. The action is called with the measured time.</param>
        /// <param name="userValue">A custom user value that will be associated to the results.
        /// Useful for tracking (for example) for which id the max or min value was recorded.
        /// </param>
        /// <returns>A disposable instance that will record the time passed until disposed.</returns>
        TimerContext NewContext(Action<TimeSpan> finalAction, string userValue = null);
    }

    /// <summary>
    /// Disposable instance used to measure time. 
    /// </summary>
    public interface TimerContext : IDisposable
    {
        /// <summary>
        /// Provides the currently elapsed time from when the instance has been created
        /// </summary>
        TimeSpan Elapsed { get; }
    }
}
