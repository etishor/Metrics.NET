
namespace Metrics
{
    /// <summary>
    /// Indicates the ability to provide the value for a metric
    /// </summary>
    /// <typeparam name="T">Type of the value returned by the metric</typeparam>
    public interface MetricValue<T> : Utils.IHideObjectMembers where T : struct
    {
        /// <summary>
        /// Get a 
        /// </summary>
        T Value { get; }
    }
}
