
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

        public void RegisterCLRAppCounters(string namePrefix = "CLR")
        {
            var clr = new CLRCounters(this.registry, namePrefix);
            clr.RegisterAppCounters();
        }

        public void RegisterCLRGlobalCounters(string namePrefix = "CLR")
        {
            var clr = new CLRCounters(this.registry, namePrefix);
            clr.RegisterAppCounters();
        }

        public void RegisterAllCLRCounters(string namePrefix = "CLR")
        {
            var clr = new CLRCounters(this.registry, namePrefix);
            clr.RegisterGlobalCounters();
            clr.RegisterAppCounters();
        }
    }
}
