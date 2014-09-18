using System;
using System.Diagnostics;

namespace Metrics
{
    /// <summary>
    /// Static wrapper around a global MetricContext instance.
    /// </summary>
    public static class Metric
    {
        private static readonly DefaultMetricsContext globalContext;
        private static readonly MetricsConfig config;

        static Metric()
        {
            globalContext = new DefaultMetricsContext(Process.GetCurrentProcess().ProcessName);
            config = new MetricsConfig(globalContext);
        }

        /// <summary>
        /// Exposes advanced operations that are possible on this metrics context.
        /// </summary>
        public static AdvancedMetricsContext Advanced { get { return globalContext; } }

        /// <summary>
        /// Create a new child metrics context. Metrics added to the child context are kept separate from the metrics in the 
        /// parent context.
        /// </summary>
        /// <param name="contextName">Name of the child context.</param>
        /// <returns>Newly created child context.</returns>
        public static MetricsContext Context(string contextName)
        {
            return globalContext.Context(contextName);
        }

        /// <summary>
        /// Create a new child metrics context. Metrics added to the child context are kept separate from the metrics in the 
        /// parent context.
        /// </summary>
        /// <param name="contextName">Name of the child context.</param>
        /// <param name="contextCreator">Function used to create the instance of the child context. (Use for creating custom contexts)</param>
        /// <returns>Newly created child context.</returns>
        public static MetricsContext Context(string contextName, Func<string, MetricsContext> contextCreator)
        {
            return globalContext.Context(contextName, contextCreator);
        }

        /// <summary>
        /// Remove a child context. The metrics for the child context are removed from the MetricsData of the parent context.
        /// </summary>
        /// <param name="contextName">Name of the child context to shutdown.</param>
        public static void ShutdownContext(string contextName)
        {
            globalContext.ShutdownContext(contextName);
        }

        /// <summary>
        /// Entrypoint for Global Metrics Configuration.
        /// </summary>
        /// <example>
        /// <code>
        /// Metric.Config
        ///     .WithHttpEndpoint("http://localhost:1234/")
        ///     .WithErrorHandler(x => Console.WriteLine(x.ToString()))
        ///     .WithPerformanceCounters(c => c.RegisterAll())
        ///     .WithReporting(config => config
        ///         .WithConsoleReport(TimeSpan.FromSeconds(30))
        ///         .WithCSVReports(@"c:\temp\reports\", TimeSpan.FromSeconds(10))
        ///         .WithTextFileReport(@"C:\temp\reports\metrics.txt", TimeSpan.FromSeconds(10))
        ///     );
        /// </code>
        /// </example>
        public static MetricsConfig Config { get { return config; } }

        /// <summary>
        /// Register a performance counter as a Gauge metric.
        /// </summary>
        /// <param name="name">Name of this gauge metric. Must be unique across all gauges in this context.</param>
        /// <param name="counterCategory">Category of the performance counter</param>
        /// <param name="counterName">Name of the performance counter</param>
        /// <param name="counterInstance">Instance of the performance counter</param>
        /// <param name="unit">Description of want the value represents ( Unit.Requests , Unit.Items etc ) .</param>
        /// <returns>Reference to the gauge</returns>
        public static void PerformanceCounter(string name, string counterCategory, string counterName, string counterInstance, Unit unit)
        {
            globalContext.PerformanceCounter(name, counterCategory, counterName, counterInstance, unit);
        }

        /// <summary>
        /// A gauge is the simplest metric type. It just returns a value. This metric is suitable for instantaneous values.
        /// </summary>
        /// <param name="name">Name of this gauge metric. Must be unique across all gauges in this context.</param>
        /// <param name="valueProvider">Function that returns the value for the gauge.</param>
        /// <param name="unit">Description of want the value represents ( Unit.Requests , Unit.Items etc ) .</param>
        /// <returns>Reference to the gauge</returns>
        public static void Gauge(string name, Func<double> valueProvider, Unit unit)
        {
            globalContext.Gauge(name, valueProvider, unit);
        }

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
        /// <param name="name">Name of the metric. Must be unique across all meters in this context.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <param name="rateUnit">Time unit for rates reporting. Defaults to Second ( occurrences / second ).</param>
        /// <returns>Reference to the metric</returns>
        public static Meter Meter(string name, Unit unit, TimeUnit rateUnit = TimeUnit.Seconds)
        {
            return globalContext.Meter(name, unit, rateUnit);
        }

        /// <summary>
        /// A counter is a simple incrementing and decrementing 64-bit integer. Ex number of active requests.
        /// </summary>
        /// <param name="name">Name of the metric. Must be unique across all counters in this context.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <returns>Reference to the metric</returns>
        public static Counter Counter(string name, Unit unit)
        {
            return globalContext.Counter(name, unit);
        }

        /// <summary>
        /// A Histogram measures the distribution of values in a stream of data: e.g., the number of results returned by a search.
        /// </summary>
        /// <param name="name">Name of the metric. Must be unique across all histograms in this context.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <param name="samplingType">Type of the sampling to use (see SamplingType for details ).</param>
        /// <returns>Reference to the metric</returns>
        public static Histogram Histogram(string name, Unit unit, SamplingType samplingType = SamplingType.FavourRecent)
        {
            return globalContext.Histogram(name, unit, samplingType);
        }

        /// <summary>
        /// A timer is basically a histogram of the duration of a type of event and a meter of the rate of its occurrence.
        /// <seealso cref="Histogram"/> and <seealso cref="Meter"/>
        /// </summary>
        /// <param name="name">Name of the metric. Must be unique across all timers in this context.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <param name="samplingType">Type of the sampling to use (see SamplingType for details ).</param>
        /// <param name="rateUnit">Time unit for rates reporting. Defaults to Second ( occurrences / second ).</param>
        /// <param name="durationUnit">Time unit for reporting durations. Defaults to Milliseconds. </param>
        /// <returns>Reference to the metric</returns>
        public static Timer Timer(string name, Unit unit, SamplingType samplingType = SamplingType.FavourRecent,
            TimeUnit rateUnit = TimeUnit.Seconds, TimeUnit durationUnit = TimeUnit.Milliseconds)
        {
            return globalContext.Timer(name, unit, samplingType, rateUnit, durationUnit);
        }
    }
}
