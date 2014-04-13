using System;
using Metrics.Core;
using Metrics.PerfCounters;
using Metrics.Reporters;

namespace Metrics
{
    /// <summary>
    /// Helper class to ease interaction with metrics
    /// </summary>
    public static class Metric
    {
        private static Lazy<MetricsRegistry> registry = new Lazy<MetricsRegistry>(() => new Registry(), true);

        private static readonly Lazy<MetricsReports> reports = new Lazy<MetricsReports>(() => new MetricsReports(Metric.Registry));
        private static readonly Lazy<PerformanceCounters> machineCounters = new Lazy<PerformanceCounters>(() => new PerformanceCounters(Metric.Registry), true);

        /// <summary>
        /// Configure the Metric static class to use a custom MetricsRegistry.
        /// </summary>
        /// <remarks>
        /// You must call Metric.ConfigureDefaultRegistry before any other Metric call.
        /// </remarks>
        /// <param name="registry">The custom registry to use for registring metrics.</param>
        public static void ConfigureDefaultRegistry(MetricsRegistry registry)
        {
            if (Metric.registry.IsValueCreated)
            {
                throw new InvalidOperationException("Metrics registry has already been created. You must call Metric.ConfigureDefaultRegistry before any other Metric call.");
            }
            Metric.registry = new Lazy<MetricsRegistry>(() => registry);
        }

        /// <summary>
        /// Global error handler for the metrics library. If a handler is registered any error will be passed to the handler.
        /// If no error handler is registered the exception will be re-thrown.
        /// </summary>
        public static Action<Exception> ErrorHandler { get; set; }

        /// <summary>
        /// Entry point for metric reporting operations
        /// </summary>
        public static MetricsReports Reports { get { return reports.Value; } }

        /// <summary>
        /// Entry point for registering metrics derived from PerformanceCounters.
        /// </summary>
        public static PerformanceCounters MachineCounters { get { return machineCounters.Value; } }

        /// <summary>
        /// The registry where all the metrics are registered. 
        /// </summary>
        public static MetricsRegistry Registry { get { return registry.Value; } }

        /// <summary>
        /// Register a performance counter as a Gauge metric.
        /// </summary>
        /// <param name="name">Name of this gauge metric. Must be unique across all gauges.</param>
        /// <param name="counterCategory">Category of the performance counter</param>
        /// <param name="counterName">Name of the performance counter</param>
        /// <param name="counterInstance">Instance of the performance counter</param>
        /// <param name="formatter">Function to format the float value returned by the counter</param>
        /// <param name="unit">Description of want the value represents ( Unit.Requests , Unit.Items etc ) .</param>
        /// <returns>Reference to the gauge</returns>
        public static Gauge PerformanceCounter(string name, string counterCategory, string counterName, string counterInstance, Func<float, string> formatter, Unit unit)
        {
            return Metric.Registry.Gauge(name, () => new PerformanceCounterGauge(counterCategory, counterName, counterInstance, formatter), unit);
        }

        /// <summary>
        /// A gauge is the simplest metric type. It just returns a value.
        /// This metric is suitable for instantaneous values. 
        /// <typeparamref name="T"/> is used as a prefix for the metric name.
        /// </summary>
        /// <typeparam name="T">Type used as a prefix for the metric name</typeparam>
        /// <param name="name">Name of this gauge metric. Must be unique across all gauges.</param>
        /// <param name="valueProvider">Function that returns the value for the gauge.</param>
        /// <param name="unit">Description of want the value represents ( Unit.Requests , Unit.Items etc ) .</param>
        /// <returns>Reference to the gauge</returns>
        public static Gauge Gauge<T>(string name, Func<string> valueProvider, Unit unit)
        {
            return Metric.Registry.Gauge(Name<T>(name), valueProvider, unit);
        }

        /// <summary>
        /// A gauge is the simplest metric type. It just returns a value. This metric is suitable for instantaneous values.
        /// </summary>
        /// <param name="name">Name of this gauge metric. Must be unique across all gauges.</param>
        /// <param name="valueProvider">Function that returns the value for the gauge.</param>
        /// <param name="unit">Description of want the value represents ( Unit.Requests , Unit.Items etc ) .</param>
        /// <returns>Reference to the gauge</returns>
        public static Gauge Gauge(string name, Func<string> valueProvider, Unit unit)
        {
            return Metric.Registry.Gauge(name, valueProvider, unit);
        }

        /// <summary>
        /// A meter measures the rate at which a set of events occur, in a few different ways. 
        /// This metric is suitable for keeping a record of now often something happens ( error, request etc ).
        /// <typeparamref name="T"/> is used as a prefix for the metric name.
        /// </summary>
        /// <remarks>
        /// The mean rate is the average rate of events. It’s generally useful for trivia, 
        /// but as it represents the total rate for your application’s entire lifetime (e.g., the total number of requests handled, 
        /// divided by the number of seconds the process has been running), it doesn’t offer a sense of recency. 
        /// Luckily, meters also record three different exponentially-weighted moving average rates: the 1-, 5-, and 15-minute moving averages.
        /// </remarks>
        /// <typeparam name="T">Type used as a prefix for the metric name</typeparam>
        /// <param name="name">Name of the metric. Must be unique across all meters.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <param name="rateUnit">Time unit for rates reporting. Defaults to Second ( occurrences / second ).</param>
        /// <returns>Reference to the metric</returns>
        public static Meter Meter<T>(string name, Unit unit, TimeUnit rateUnit = TimeUnit.Seconds)
        {
            return Meter(Name<T>(name), unit, rateUnit);
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
        /// <param name="name">Name of the metric. Must be unique across all meters.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <param name="rateUnit">Time unit for rates reporting. Defaults to Second ( occurrences / second ).</param>
        /// <returns>Reference to the metric</returns>
        public static Meter Meter(string name, Unit unit, TimeUnit rateUnit = TimeUnit.Seconds)
        {
            return Metric.Registry.Meter(name, unit, rateUnit);
        }

        /// <summary>
        /// A counter is a simple incrementing and decrementing 64-bit integer. Ex number of active requests.
        /// <typeparamref name="T"/> is used as a prefix for the metric name.
        /// </summary>
        /// <typeparam name="T">Type used as a prefix for the metric name</typeparam>
        /// <param name="name">Name of the metric. Must be unique across all counters.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <returns>Reference to the metric</returns>
        public static Counter Counter<T>(string name, Unit unit)
        {
            return Counter(Name<T>(name), unit);
        }

        /// <summary>
        /// A counter is a simple incrementing and decrementing 64-bit integer. Ex number of active requests.
        /// </summary>
        /// <param name="name">Name of the metric. Must be unique across all counters.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <returns>Reference to the metric</returns>
        public static Counter Counter(string name, Unit unit)
        {
            return Metric.Registry.Counter(name, unit);
        }

        /// <summary>
        /// A Histogram measures the distribution of values in a stream of data: e.g., the number of results returned by a search.
        /// <typeparamref name="T"/> is used as a prefix for the metric name.
        /// </summary>
        /// <typeparam name="T">Type used as a prefix for the metric name</typeparam>
        /// <param name="name">Name of the metric. Must be unique across all histograms.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <param name="samplingType">Type of the sampling to use (see SamplingType for details ).</param>
        /// <returns>Reference to the metric</returns>
        public static Histogram Histogram<T>(string name, Unit unit, SamplingType samplingType = SamplingType.FavourRecent)
        {
            return Histogram(Name<T>(name), unit, samplingType);
        }

        /// <summary>
        /// A Histogram measures the distribution of values in a stream of data: e.g., the number of results returned by a search.
        /// </summary>
        /// <param name="name">Name of the metric. Must be unique across all histograms.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <param name="samplingType">Type of the sampling to use (see SamplingType for details ).</param>
        /// <returns>Reference to the metric</returns>
        public static Histogram Histogram(string name, Unit unit, SamplingType samplingType = SamplingType.FavourRecent)
        {
            return Metric.Registry.Histogram(name, unit, samplingType);
        }

        /// <summary>
        /// A timer is basically a histogram of the duration of a type of event and a meter of the rate of its occurrence.
        /// <seealso cref="Histogram"/> and <seealso cref="Meter"/>
        /// <typeparamref name="T"/> is used as a prefix for the metric name.
        /// </summary>
        /// <typeparam name="T">Type used as a prefix for the metric name</typeparam>
        /// <param name="samplingType">Type of the sampling to use (see SamplingType for details ).</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <param name="name">Name of the metric. Must be unique across all counters.</param>
        /// <param name="rateUnit">Time unit for rates reporting. Defaults to Second ( occurrences / second ).</param>
        /// <param name="durationUnit">Time unit for reporting durations. Defaults to Milliseconds. </param>
        /// <returns>Reference to the metric</returns>
        public static Timer Timer<T>(string name, Unit unit, SamplingType samplingType = SamplingType.FavourRecent,
             TimeUnit rateUnit = TimeUnit.Seconds, TimeUnit durationUnit = TimeUnit.Milliseconds)
        {
            return Timer(Name<T>(name), unit, samplingType, rateUnit, durationUnit);
        }

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
        public static Timer Timer(string name, Unit unit, SamplingType samplingType = SamplingType.FavourRecent,
            TimeUnit rateUnit = TimeUnit.Seconds, TimeUnit durationUnit = TimeUnit.Milliseconds)
        {
            return Metric.Registry.Timer(name, unit, samplingType, rateUnit, durationUnit);
        }

        /// <summary>
        /// Returns an anonymous object representing all the values for all the registered metrics.
        /// This object is suitable for JSON/XML etc serialization. 
        /// </summary>
        /// <returns>Object suitable for serialization.</returns>
        public static object GetForSerialization()
        {
            return RegistrySerializer.GetForSerialization(Metric.Registry);
        }

        /// <summary>
        /// Returns a string containing a human readable formatted report with all the metrics.
        /// </summary>
        /// <returns>String containing the report.</returns>
        public static string GetAsHumanReadable()
        {
            var report = new StringReporter();
            report.RunReport(Metric.Registry);
            return report.Result;
        }

        private static string Name<T>(string name)
        {
            return string.Concat(typeof(T).Name, ".", name);
        }
    }
}
