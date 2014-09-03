using System;
namespace Metrics
{
    /// <summary>
    /// Represents a logical grouping of metrics
    /// </summary>
    public interface MetricsContext : IDisposable, Utils.IHideObjectMembers
    {
        /// <summary>
        /// Returns a metrics data provider capable of returning the metrics in this context and any existing child contexts.
        /// </summary>
        MetricsDataProvider DataProvider { get; }

        /// <summary>
        /// Create a new child metrics context. Metrics added to the child context are kept separate from the metrics in the 
        /// parent context.
        /// </summary>
        /// <param name="contextName">Name of the child context.</param>
        /// <returns>Newly created child context.</returns>
        MetricsContext Context(string contextName);

        /// <summary>
        /// Create a new child metrics context. Metrics added to the child context are kept separate from the metrics in the 
        /// parent context.
        /// </summary>
        /// <param name="contextName">Name of the child context.</param>
        /// <param name="contextCreator">Function used to create the instance of the child context. (Use for creating custom contexts)</param>
        /// <returns>Newly created child context.</returns>
        MetricsContext Context(string contextName, Func<string, MetricsContext> contextCreator);

        /// <summary>
        /// Remove a child context. The metrics for the child context are removed from the MetricsData of the parent context.
        /// </summary>
        /// <param name="contextName">Name of the child context to shutdown.</param>
        void ShutdownContext(string contextName);

        /// <summary>
        /// A gauge is the simplest metric type. It just returns a value. This metric is suitable for instantaneous values.
        /// </summary>
        /// <param name="name">Name of this gauge metric. Must be unique across all gauges.</param>
        /// <param name="valueProvider">Function that returns the value for the gauge.</param>
        /// <param name="unit">Description of want the value represents ( Unit.Requests , Unit.Items etc ) .</param>
        /// <returns>Reference to the gauge</returns>
        Gauge Gauge(string name, Func<double> valueProvider, Unit unit);

        /// <summary>
        /// Register a performance counter as a Gauge metric.
        /// </summary>
        /// <param name="name">Name of this gauge metric. Must be unique across all gauges.</param>
        /// <param name="counterCategory">Category of the performance counter</param>
        /// <param name="counterName">Name of the performance counter</param>
        /// <param name="counterInstance">Instance of the performance counter</param>
        /// <param name="unit">Description of want the value represents ( Unit.Requests , Unit.Items etc ) .</param>
        /// <returns>Reference to the gauge</returns>
        Gauge PerformanceCounter(string name, string counterCategory, string counterName, string counterInstance, Unit unit);

        /// <summary>
        /// A counter is a simple incrementing and decrementing 64-bit integer. Ex number of active requests.
        /// </summary>
        /// <param name="name">Name of the metric. Must be unique across all counters.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <returns>Reference to the metric</returns>
        Counter Counter(string name, Unit unit);

        /// <summary>
        /// A meter measures the rate at which a set of events occur, in a few different ways. 
        /// This metric is suitable for keeping a record of now often something happens ( error, request etc ).
        /// </summary>
        /// <remarks>
        /// The mean rate is the average rate of events. It’s generally useful for trivia, 
        /// but as it represents the total rate for your application’s entire lifetime (e.g., the total number of requests handled, 
        /// divided by the number of seconds the process has been running), it doesn’t offer a sense of recency. 
        /// Luckily, meters also record three different exponentially-weighted moving average rates: the 1-, 5-, and 15-minute moving averages.
        /// </remarks>
        /// <param name="name">Name of the metric. Must be unique across all meters.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <param name="rateUnit">Time unit for rates reporting. Defaults to Second ( occurrences / second ).</param>
        /// <returns>Reference to the metric</returns>
        Meter Meter(string name, Unit unit, TimeUnit rateUnit = TimeUnit.Seconds);

        /// <summary>
        /// A Histogram measures the distribution of values in a stream of data: e.g., the number of results returned by a search.
        /// </summary>
        /// <param name="name">Name of the metric. Must be unique across all histograms.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <param name="samplingType">Type of the sampling to use (see SamplingType for details ).</param>
        /// <returns>Reference to the metric</returns>
        Histogram Histogram(string name, Unit unit, SamplingType samplingType = SamplingType.FavourRecent);

        /// <summary>
        /// A timer is basically a histogram of the duration of a type of event and a meter of the rate of its occurrence.
        /// <seealso cref="Histogram"/> and <seealso cref="Meter"/>
        /// </summary>
        /// <param name="name">Name of the metric. Must be unique across all counters.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <param name="samplingType">Type of the sampling to use (see SamplingType for details ).</param>
        /// <param name="rateUnit">Time unit for rates reporting. Defaults to Second ( occurrences / second ).</param>
        /// <param name="durationUnit">Time unit for reporting durations. Defaults to Milliseconds. </param>
        /// <returns>Reference to the metric</returns>
        Timer Timer(string name, Unit unit, SamplingType samplingType = SamplingType.FavourRecent, TimeUnit rateUnit = TimeUnit.Seconds, TimeUnit durationUnit = TimeUnit.Milliseconds);

        /// <summary>
        /// All metrics operations will be NO-OP.
        /// This is useful for measuring the impact of the metrics library on the application.
        /// If you think the Metrics library is causing issues, this will disable all Metrics operations.
        /// </summary>
        void CompletelyDisableMetrics();

        /// <summary>
        /// Event fired when the context is disposed or shutdown or the CompletelyDisableMetrics is called.
        /// </summary>
        event EventHandler ContextShuttingDown;
    }
}
