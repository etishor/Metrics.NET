
using System;
using System.Diagnostics;
using Metrics.Core;
namespace Metrics.PerfCounters
{
    public static class PerformanceCounters
    {
        private const string TotalInstance = "_Total";
        private const string GlobalInstance = "_Global_";

        private const string Exceptions = ".NET CLR Exceptions";
        private const string Memory = ".NET CLR Memory";
        private const string LocksAndThreads = ".NET CLR LocksAndThreads";

        public static void RegisterSystemCounters(MetricsContext registry)
        {
            registry.Register("System AvailableRAM", () => new PerformanceCounterGauge("Memory", "Available MBytes"), Unit.Custom("Mb"));
            registry.Register("System CPU Usage", () => new PerformanceCounterGauge("Processor", "% Processor Time", TotalInstance), Unit.Custom("%"));
            registry.Register("System Disk Writes/sec", () => new DerivedGauge(new PerformanceCounterGauge("PhysicalDisk", "Disk Reads/sec", TotalInstance), f => f / 1024), Unit.Custom("kb/s"));
            registry.Register("System Disk Reads/sec", () => new DerivedGauge(new PerformanceCounterGauge("PhysicalDisk", "Disk Writes/sec", TotalInstance), f => f / 1024), Unit.Custom("kb/s"));
        }

        public static void RegisterCLRGlobalCounters(MetricsContext registry)
        {
            registry.Register(".NET Mb in all Heaps", () => new DerivedGauge(new PerformanceCounterGauge(Memory, "# Bytes in all Heaps", GlobalInstance), v => v / (1024 * 1024)), Unit.Custom("Mb"));
            registry.Register(".NET Time in GC", () => new PerformanceCounterGauge(Memory, "% Time in GC", GlobalInstance), Unit.Custom("%"));
        }

        public static void RegisterCLRAppCounters(MetricsContext registry)
        {
            var app = Process.GetCurrentProcess().ProcessName;
            registry.Register("Mb in all Heaps", () => new DerivedGauge(new PerformanceCounterGauge(Memory, "# Bytes in all Heaps", app), v => v / (1024 * 1024)), Unit.Custom("Mb"));
            registry.Register("Time in GC", () => new PerformanceCounterGauge(Memory, "% Time in GC", app), Unit.Custom("%"));
            registry.Register("Total Exceptions", () => new PerformanceCounterGauge(Exceptions, "# of Exceps Thrown", app), Unit.Custom("Exceptions"));
            registry.Register("Exceptions Thrown / Sec", () => new PerformanceCounterGauge(Exceptions, "# of Exceps Thrown / Sec", app), Unit.Custom("Exceptions/s"));
            registry.Register("Logical Threads", () => new PerformanceCounterGauge(LocksAndThreads, "# of current logical Threads", app), Unit.Custom("Threads"));
            registry.Register("Physical Threads", () => new PerformanceCounterGauge(LocksAndThreads, "# of current physical Threads", app), Unit.Custom("Threads"));
            registry.Register("Contention Rate / Sec", () => new PerformanceCounterGauge(LocksAndThreads, "Contention Rate / Sec", app), Unit.Custom("Attempts/s"));
            registry.Register("Queue Length / sec", () => new PerformanceCounterGauge(LocksAndThreads, "Queue Length / sec", app), Unit.Custom("Threads/s"));
        }

        private static void Register(this MetricsContext registry, string name, Func<GaugeMetric> gauge, Unit unit)
        {
            registry.Gauge(name, () => gauge().Value, unit);
        }
    }
}
