
namespace Metrics
{
    public interface MetricsDataProvider : Utils.IHideObjectMembers
    {
        MetricsData CurrentMetricsData { get; }
    }
}
