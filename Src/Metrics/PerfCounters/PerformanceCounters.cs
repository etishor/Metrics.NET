
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
            RegisterAllCLRCounters();
        }

        public void RegisterSystemInfo(string namePrefix = "System")
        {
            new SystemInfo(this.registry, namePrefix).Register();
        }

        public void RegisterCLRAppCounters(string namePrefix = "App")
        {
            var clr = new CLRCounters(this.registry, namePrefix);
            clr.RegisterAppCounters();
        }

        public void RegisterCLRGlobalCounters(string namePrefix = "System")
        {
            var clr = new CLRCounters(this.registry, namePrefix);
            clr.RegisterAppCounters();
        }

        public void RegisterAllCLRCounters()
        {
            RegisterCLRGlobalCounters();
            RegisterCLRAppCounters();
        }
    }
}
