

namespace Metrics.Core
{
    public interface Reservoir
    {
        int Size { get; }
        void Update(long value);
        Snapshot Snapshot { get; }
    }
}
