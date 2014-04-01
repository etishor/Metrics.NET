
using System.Diagnostics;
namespace Metrics.PerfCounters
{
    public class CLRCounters : BaseCounterRegristry
    {
        private const string Exceptions = ".NET CLR Exceptions";
        private const string Memory = ".NET CLR Memory";
        private const string LocksAndThreads = ".NET CLR LocksAndThreads";

        public CLRCounters(MetricsRegistry registry, string prefix)
            : base(registry, prefix) { }

        public void RegisterGlobalCounters()
        {
            Register("System.Mb in all Heaps", () => new PerformanceCounterGauge(Memory, "# Bytes in all Heaps", GlobalInstance, f => (f / (1024 * 1024)).ToString("F")), Unit.Custom("Mb"));
            Register("System.Time in GC", () => new PerformanceCounterGauge(Memory, "% Time in GC", GlobalInstance), Unit.Custom("%"));
        }

        public void RegisterAppCounters()
        {
            var app = Process.GetCurrentProcess().ProcessName;
            Register(app + ".Mb in all Heaps", () => new PerformanceCounterGauge(Memory, "# Bytes in all Heaps", app, f => (f / (1024 * 1024)).ToString("F")), Unit.Custom("Mb"));
            Register(app + ".Time in GC", () => new PerformanceCounterGauge(Memory, "% Time in GC", app), Unit.Custom("%"));
            Register(app + ".Total Exceptions", () => new PerformanceCounterGauge(Exceptions, "# of Exceps Thrown", app), Unit.Custom("Exceptions"));
            Register(app + ".Exceptions Thrown / Sec", () => new PerformanceCounterGauge(Exceptions, "# of Exceps Thrown / Sec", app), Unit.Custom("Exceptions/s"));
            Register(app + ".Logical Threads", () => new PerformanceCounterGauge(LocksAndThreads, "# of current logical Threads", app), Unit.Custom("Threads"));
            Register(app + ".Physical Threads", () => new PerformanceCounterGauge(LocksAndThreads, "# of current physical Threads", app), Unit.Custom("Threads"));
            Register(app + ".Contention Rate / Sec", () => new PerformanceCounterGauge(LocksAndThreads, "Contention Rate / Sec", app), Unit.Custom("Attempts/s"));
            Register(app + ".Queue Length / sec", () => new PerformanceCounterGauge(LocksAndThreads, "Queue Length / sec", app), Unit.Custom("Threads/s"));
        }

    }
}
