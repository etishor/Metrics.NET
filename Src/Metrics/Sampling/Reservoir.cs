

namespace Metrics.Sampling
{
    public interface Reservoir
    {
        int Size { get; }
        void Update(long value, string userValue = null);
        Snapshot Snapshot { get; }
        void Reset();
    }
}
