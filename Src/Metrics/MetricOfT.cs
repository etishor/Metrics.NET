
namespace Metrics
{
    /// <summary>
    /// Marker interface for metric classes
    /// </summary>
    /// <typeparam name="T">Type of the value returned by the metric</typeparam>
    public interface Metric<T> : Utils.IHideObjectMembers where T : struct
    {
        /// <summary>
        /// Get a 
        /// </summary>
        T Value { get; }
    }
}
