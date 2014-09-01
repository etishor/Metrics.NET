
using System;
using System.Collections.Concurrent;
using System.Linq;
using Metrics.Core;
using Metrics.PerfCounters;
namespace Metrics
{
    public sealed class MetricsContext : IDisposable
    {
        private readonly ConcurrentDictionary<string, MetricsContext> childContexts = new ConcurrentDictionary<string, MetricsContext>();

        private readonly string context;
        private readonly MetricsConfig config;

        private MetricsRegistry registry;
        private bool isDisabled;

        public MetricsContext()
            : this(string.Empty) { }

        public MetricsContext(string context)
            : this(context, new LocalRegistry(context))
        { }

        public MetricsContext(string context, MetricsRegistry registry)
        {
            this.context = context;
            this.registry = registry;
            this.config = new MetricsConfig(this);
        }

        public MetricsContext Context(string contextName, Func<string, MetricsContext> contextCreator)
        {
            if (this.isDisabled)
            {
                return this;
            }
            return this.childContexts.GetOrAdd(contextName, contextCreator);
        }

        public MetricsContext Context(string contextName)
        {
            if (string.IsNullOrEmpty(contextName))
            {
                return this;
            }

            return this.Context(contextName, c => new MetricsContext(contextName));
        }

        public void ShutdownContext(string contextName)
        {
            if (string.IsNullOrEmpty(contextName))
            {
                throw new ArgumentException("contextName must not be null or empty", contextName);
            }

            MetricsContext context;
            if (this.childContexts.TryRemove(contextName, out context))
            {
                using (context) { }
            }
        }

        public MetricsData MetricsData
        {
            get
            {
                if (this.isDisabled)
                {
                    return MetricsData.Empty;
                }

                return new MetricsData(this.context, this.registry.MetricsData, this.childContexts.Values.Select(c => c.MetricsData));
            }
        }

        public MetricsConfig Config
        {
            get
            {
                return this.config;
            }
        }

        /// <summary>
        /// Register a performance counter as a Gauge metric.
        /// </summary>
        /// <param name="name">Name of this gauge metric. Must be unique across all gauges.</param>
        /// <param name="counterCategory">Category of the performance counter</param>
        /// <param name="counterName">Name of the performance counter</param>
        /// <param name="counterInstance">Instance of the performance counter</param>
        /// <param name="unit">Description of want the value represents ( Unit.Requests , Unit.Items etc ) .</param>
        /// <returns>Reference to the gauge</returns>
        public Gauge PerformanceCounter(string name, string counterCategory, string counterName, string counterInstance, Unit unit)
        {
            return this.registry.Gauge(name, () => new PerformanceCounterGauge(counterCategory, counterName, counterInstance), unit);
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
        public Gauge Gauge<T>(string name, Func<double> valueProvider, Unit unit)
        {
            return this.registry.Gauge(Name<T>(name), valueProvider, unit);
        }

        /// <summary>
        /// A gauge is the simplest metric type. It just returns a value. This metric is suitable for instantaneous values.
        /// </summary>
        /// <param name="name">Name of this gauge metric. Must be unique across all gauges.</param>
        /// <param name="valueProvider">Function that returns the value for the gauge.</param>
        /// <param name="unit">Description of want the value represents ( Unit.Requests , Unit.Items etc ) .</param>
        /// <returns>Reference to the gauge</returns>
        public Gauge Gauge(string name, Func<double> valueProvider, Unit unit)
        {
            return this.registry.Gauge(name, valueProvider, unit);
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
        public Meter Meter<T>(string name, Unit unit, TimeUnit rateUnit = TimeUnit.Seconds)
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
        public Meter Meter(string name, Unit unit, TimeUnit rateUnit = TimeUnit.Seconds)
        {
            return this.registry.Meter(name, unit, rateUnit);
        }

        /// <summary>
        /// A counter is a simple incrementing and decrementing 64-bit integer. Ex number of active requests.
        /// <typeparamref name="T"/> is used as a prefix for the metric name.
        /// </summary>
        /// <typeparam name="T">Type used as a prefix for the metric name</typeparam>
        /// <param name="name">Name of the metric. Must be unique across all counters.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <returns>Reference to the metric</returns>
        public Counter Counter<T>(string name, Unit unit)
        {
            return Counter(Name<T>(name), unit);
        }

        /// <summary>
        /// A counter is a simple incrementing and decrementing 64-bit integer. Ex number of active requests.
        /// </summary>
        /// <param name="name">Name of the metric. Must be unique across all counters.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <returns>Reference to the metric</returns>
        public Counter Counter(string name, Unit unit)
        {
            return this.registry.Counter(name, unit);
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
        public Histogram Histogram<T>(string name, Unit unit, SamplingType samplingType = SamplingType.FavourRecent)
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
        public Histogram Histogram(string name, Unit unit, SamplingType samplingType = SamplingType.FavourRecent)
        {
            return this.registry.Histogram(name, unit, samplingType);
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
        public Timer Timer<T>(string name, Unit unit, SamplingType samplingType = SamplingType.FavourRecent,
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
        public Timer Timer(string name, Unit unit, SamplingType samplingType = SamplingType.FavourRecent,
            TimeUnit rateUnit = TimeUnit.Seconds, TimeUnit durationUnit = TimeUnit.Milliseconds)
        {
            return this.registry.Timer(name, unit, samplingType, rateUnit, durationUnit);
        }

        /// <summary>
        /// All metrics operations will be NO-OP.
        /// This is useful for measuring the impact of the metrics library on the application.
        /// If you think the Metrics library is causing issues, this will disable all Metrics operations.
        /// </summary>
        public void CompletelyDisableMetrics()
        {
            if (this.isDisabled)
            {
                return;
            }

            this.isDisabled = true;
            var oldRegistry = this.registry;
            this.registry = new NullMetricsRegistry();
            this.config.DisableAllReports();

            foreach (var context in this.childContexts.Values)
            {
                context.CompletelyDisableMetrics();
            }
        }

        public void Dispose()
        {
            using (this.config) { }
        }

        private string Name<T>(string name)
        {
            return string.Concat(typeof(T).Name, ".", name);
        }
    }
}
