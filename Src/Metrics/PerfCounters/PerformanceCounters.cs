
namespace Metrics.PerfCounters
{
    public class PerformanceCounters
    {
        private MetricsRegistry registry;

        public PerformanceCounters(MetricsRegistry registry)
        {
            this.registry = registry;
        }

        public void RegisterAll()
        {
            RegisterSystemInfo();
        }

        public void RegisterSystemInfo(string namePrefix = "System")
        {
            new SystemInfo(this.registry, namePrefix).Register();
        }
    }
}
