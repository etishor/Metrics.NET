
namespace Metrics
{
    /// <summary>
    /// Indicates the ability to provide the value for a metric.
    /// This is the raw value. Consumers should use <see cref="MetricValueSource{T}"/>
    /// </summary>
    /// <typeparam name="T">Type of the value returned by the metric</typeparam>
    public interface MetricValueProvider<T> : Utils.IHideObjectMembers where T : struct
    {
        /// <summary>
        /// The current value of the metric.
        /// </summary>
        T Value { get; }
    }

    /// <summary>
    /// Provides the value of a metric and information about units.
    /// This is the class that metric consumers should use.
    /// </summary>
    /// <typeparam name="T">Type of the metric value</typeparam>
    public abstract class MetricValueSource<T> : Utils.IHideObjectMembers
        where T : struct
    {
        private readonly MetricValueProvider<T> valueProvider;

        protected MetricValueSource(string name, MetricValueProvider<T> valueProvider, Unit unit, MetricTags tags)
        {
            this.Name = name;
            this.Unit = unit;
            this.valueProvider = valueProvider;
            this.Tags = tags.Tags;
        }

        /// <summary>
        /// Name of the metric.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The current value of the metric.
        /// </summary>
        public T Value { get { return this.valueProvider.Value; } }

        /// <summary>
        /// Unit representing what the metric is measuring.
        /// </summary>
        public Unit Unit { get; private set; }

        /// <summary>
        /// Tags associated with the metric.
        /// </summary>
        public string[] Tags { get; private set; }
    }
}
