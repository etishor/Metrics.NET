using System;
using Metrics.Core;
using Metrics.Reporters;

namespace Metrics
{
    /// <summary>
    /// Helper class to ease interaction with metrics
    /// </summary>
    public static class Metric
    {
        private static readonly MetricsRegistry registry = new Registry();
        private static readonly MetricsReports reports = new MetricsReports(registry);

        /// <summary>
        /// Entry point for metric reporting operations
        /// </summary>
        public static MetricsReports Reports { get { return reports; } }

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
            return registry.Gauge(Name<T>(name), valueProvider, unit);
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
            return registry.Gauge(name, valueProvider, unit);
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
            return registry.Meter(name, unit, rateUnit);
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
            return registry.Counter(name, unit);
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
            return registry.Histogram(name, unit, samplingType);
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
            return registry.Timer(name, unit, samplingType, rateUnit, durationUnit);
        }

        /// <summary>
        /// Returns an anonymous object representing all the values for all the registered metrics.
        /// This object is suitable for JSON/XML etc serialization. 
        /// </summary>
        /// <returns>Object suitable for serialization.</returns>
        public static object GetForSerialization()
        {
            return new RegistrySerializer().GetForSerialization(registry);
        }

        /// <summary>
        /// Returns a string containing a human readable formatted report with all the metrics.
        /// </summary>
        /// <returns>String containing the report.</returns>
        public static string GetAsHumanReadable()
        {
            using (var report = new StringReporter())
            {
                report.RunReport(registry);
                return report.Result;
            }
        }

        private static string Name<T>(string name)
        {
            return string.Concat(typeof(T).Name, ".", name);
        }
    }
}
