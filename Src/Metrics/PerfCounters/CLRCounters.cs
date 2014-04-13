
using System.Diagnostics;
using Metrics.Core;
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
            Register("Mb in all Heaps", () => new DerivedGauge(new PerformanceCounterGauge(Memory, "# Bytes in all Heaps", GlobalInstance), v => v / (1024 * 1024)), Unit.Custom("Mb"));
            Register("Time in GC", () => new PerformanceCounterGauge(Memory, "% Time in GC", GlobalInstance), Unit.Custom("%"));
        }

        public void RegisterAppCounters()
        {
            var app = Process.GetCurrentProcess().ProcessName;
            Register("Mb in all Heaps", () => new DerivedGauge(new PerformanceCounterGauge(Memory, "# Bytes in all Heaps", app), v => v / (1024 * 1024)), Unit.Custom("Mb"));
            Register("Time in GC", () => new PerformanceCounterGauge(Memory, "% Time in GC", app), Unit.Custom("%"));
            Register("Total Exceptions", () => new PerformanceCounterGauge(Exceptions, "# of Exceps Thrown", app), Unit.Custom("Exceptions"));
            Register("Exceptions Thrown / Sec", () => new PerformanceCounterGauge(Exceptions, "# of Exceps Thrown / Sec", app), Unit.Custom("Exceptions/s"));
            Register("Logical Threads", () => new PerformanceCounterGauge(LocksAndThreads, "# of current logical Threads", app), Unit.Custom("Threads"));
            Register("Physical Threads", () => new PerformanceCounterGauge(LocksAndThreads, "# of current physical Threads", app), Unit.Custom("Threads"));
            Register("Contention Rate / Sec", () => new PerformanceCounterGauge(LocksAndThreads, "Contention Rate / Sec", app), Unit.Custom("Attempts/s"));
            Register("Queue Length / sec", () => new PerformanceCounterGauge(LocksAndThreads, "Queue Length / sec", app), Unit.Custom("Threads/s"));
        }
    }
}
