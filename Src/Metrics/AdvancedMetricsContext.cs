using System;
using Metrics.Core;

namespace Metrics
{
    public interface AdvancedMetricsContext
    {
        /// <summary>
        /// All metrics operations will be NO-OP.
        /// This is useful for measuring the impact of the metrics library on the application.
        /// If you think the Metrics library is causing issues, this will disable all Metrics operations.
        /// </summary>
        void CompletelyDisableMetrics();

        /// <summary>
        /// Clear all collected data for all the metrics in this context
        /// </summary>
        void ResetMetricsValues();

        /// <summary>
        /// Event fired when the context is disposed or shutdown or the CompletelyDisableMetrics is called.
        /// </summary>
        event EventHandler ContextShuttingDown;

        /// <summary>
        /// Event fired when the context CompletelyDisableMetrics is called.
        /// </summary>
        event EventHandler ContextDisabled;

        /// <summary>
        /// Register a custom Gauge instance.
        /// </summary>
        /// <param name="name">Name of the metric. Must be unique across all counters in this context.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <param name="valueProvider">Function used to build a custom instance.</param>
        void Gauge(string name, Func<MetricValueProvider<double>> valueProvider, Unit unit);

        /// <summary>
        /// Register a custom Counter instance
        /// </summary>
        /// <param name="name">Name of the metric. Must be unique across all counters in this context.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <param name="builder">Function used to build a custom instance.</param>
        /// <returns>Reference to the metric</returns>
        Counter Counter<T>(string name, Unit unit, Func<T> builder)
            where T : Counter, MetricValueProvider<long>;

        /// <summary>
        /// Register a custom Meter instance.
        /// </summary>
        /// <param name="name">Name of the metric. Must be unique across all meters in this context.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <param name="builder">Function used to build a custom instance.</param>
        /// <param name="rateUnit">Time unit for rates reporting. Defaults to Second ( occurrences / second ).</param>
        /// <returns>Reference to the metric</returns>
        Meter Meter<T>(string name, Unit unit, Func<T> builder, TimeUnit rateUnit = TimeUnit.Seconds)
            where T : Meter, MetricValueProvider<MeterValue>;

        /// <summary>
        /// Register a custom Histogram instance
        /// </summary>
        /// <param name="name">Name of the metric. Must be unique across all histograms in this context.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <param name="builder">Function used to build a custom instance.</param>
        /// <returns>Reference to the metric</returns>
        Histogram Histogram<T>(string name, Unit unit, Func<T> builder)
            where T : Histogram, MetricValueProvider<HistogramValue>;

        /// <summary>
        /// Register a Histogram metric with a custom Reservoir instance
        /// </summary>
        /// <param name="name">Name of the metric. Must be unique across all histograms in this context.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <param name="builder">Function used to build a custom reservoir instance.</param>
        /// <returns>Reference to the metric</returns>
        Histogram Histogram(string name, Unit unit, Func<Reservoir> builder);

        /// <summary>
        /// Register a custom Timer implementation.
        /// </summary>
        /// <param name="name">Name of the metric. Must be unique across all timers in this context.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <param name="builder">Function used to build a custom instance.</param>
        /// <param name="rateUnit">Time unit for rates reporting. Defaults to Second ( occurrences / second ).</param>
        /// <param name="durationUnit">Time unit for reporting durations. Defaults to Milliseconds. </param>
        /// <returns>Reference to the metric</returns>
        Timer Timer<T>(string name, Unit unit, Func<T> builder, TimeUnit rateUnit = TimeUnit.Seconds, TimeUnit durationUnit = TimeUnit.Milliseconds)
            where T : Timer, MetricValueProvider<TimerValue>;

        /// <summary>
        /// Register a Timer metric with a custom Histogram implementation.
        /// </summary>
        /// <param name="name">Name of the metric. Must be unique across all timers in this context.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <param name="builder">Function used to build a custom histogram instance.</param>
        /// <param name="rateUnit">Time unit for rates reporting. Defaults to Second ( occurrences / second ).</param>
        /// <param name="durationUnit">Time unit for reporting durations. Defaults to Milliseconds. </param>
        /// <returns>Reference to the metric</returns>
        Timer Timer(string name, Unit unit, Func<Histogram> builder, TimeUnit rateUnit = TimeUnit.Seconds, TimeUnit durationUnit = TimeUnit.Milliseconds);

        /// <summary>
        /// Register a Timer metric with a custom Reservoir implementation for the histogram.
        /// </summary>
        /// <param name="name">Name of the metric. Must be unique across all timers in this context.</param>
        /// <param name="unit">Description of what the is being measured ( Unit.Requests , Unit.Items etc ) .</param>
        /// <param name="builder">Function used to build a custom reservoir instance.</param>
        /// <param name="rateUnit">Time unit for rates reporting. Defaults to Second ( occurrences / second ).</param>
        /// <param name="durationUnit">Time unit for reporting durations. Defaults to Milliseconds. </param>
        /// <returns>Reference to the metric</returns>
        Timer Timer(string name, Unit unit, Func<Reservoir> builder, TimeUnit rateUnit = TimeUnit.Seconds, TimeUnit durationUnit = TimeUnit.Milliseconds);

        /// <summary>
        /// Replace the DefaultMetricsBuilder used in this context.
        /// </summary>
        /// <param name="metricsBuilder">The custom metrics builder.</param>
        void WithCustomMetricsBuilder(MetricsBuilder metricsBuilder);
    }
}
