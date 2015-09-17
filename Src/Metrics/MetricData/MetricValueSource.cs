﻿
using System;
namespace Metrics.MetricData
{
    /// <summary>
    /// Indicates the ability to provide the value for a metric.
    /// This is the raw value. Consumers should use <see cref="MetricValueSource{T}"/>
    /// </summary>
    /// <typeparam name="T">Type of the value returned by the metric</typeparam>
    public interface MetricValueProvider<T> : Utils.IHideObjectMembers
    {
        /// <summary>
        /// The current value of the metric.
        /// </summary>
        T Value { get; }

        /// <summary>
        /// Get the current value for the metric, but also reset the metric.
        /// Useful for pushing data to only one consumer (ex: graphite) where you might want to only capture values just between the report interval.
        /// </summary>
        /// <param name="resetMetric">if set to true the metric will be reset.</param>
        /// <returns>The current value for the metric.</returns>
        T GetValue(bool resetMetric = false);
    }

    public sealed class ScaledValueProvider<T> : MetricValueProvider<T>
    {
        private readonly MetricValueProvider<T> valueProvider;
        private readonly Func<T, T> scalingFunction;

        public ScaledValueProvider(MetricValueProvider<T> valueProvider, Func<T, T> transformation)
        {
            this.valueProvider = valueProvider;
            this.scalingFunction = transformation;
        }

        public T Value
        {
            get
            {
                return this.scalingFunction(this.valueProvider.Value);
            }
        }

        public T GetValue(bool resetMetric = false)
        {
            return this.scalingFunction(this.valueProvider.GetValue(resetMetric));
        }

        public MetricValueProvider<T> ValueProvider
        {
            get { return this.valueProvider; }
        }
    }

    /// <summary>
    /// Provides the value of a metric and information about units.
    /// This is the class that metric consumers should use.
    /// </summary>
    /// <typeparam name="T">Type of the metric value</typeparam>
    public abstract class MetricValueSource<T> : Utils.IHideObjectMembers
    {
        protected MetricValueSource(string name, MetricValueProvider<T> valueProvider, Unit unit, MetricTags tags)
        {
            this.Name = name;
            this.Unit = unit;
            this.ValueProvider = valueProvider;
            this.Tags = tags.Tags;
        }

        /// <summary>
        /// Name of the metric.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The current value of the metric.
        /// </summary>
        public T Value { get { return this.ValueProvider.Value; } }

        /// <summary>
        /// Unit representing what the metric is measuring.
        /// </summary>
        public Unit Unit { get; private set; }

        /// <summary>
        /// Tags associated with the metric.
        /// </summary>
        public string[] Tags { get; private set; }

        /// <summary>
        /// Instance capable of returning the current value for the metric.
        /// </summary>
        public MetricValueProvider<T> ValueProvider { get; }
    }
}
