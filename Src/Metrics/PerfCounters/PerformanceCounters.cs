
using System;
using System.Diagnostics;
using Metrics.Core;
using Metrics.MetricData;
namespace Metrics.PerfCounters
{
    public static class PerformanceCounters
    {
        private const string TotalInstance = "_Total";
        private const string GlobalInstance = "_Global_";

        private const string Exceptions = ".NET CLR Exceptions";
        private const string Memory = ".NET CLR Memory";
        private const string LocksAndThreads = ".NET CLR LocksAndThreads";

        public static void RegisterSystemCounters(MetricsContext context)
        {
            context.Register("System AvailableRAM", () => new PerformanceCounterGauge("Memory", "Available MBytes"), Unit.Custom("Mb"));
            context.Register("System CPU Usage", () => new PerformanceCounterGauge("Processor", "% Processor Time", TotalInstance), Unit.Custom("%"));
            context.Register("System Disk Writes/sec", () => new DerivedGauge(new PerformanceCounterGauge("PhysicalDisk", "Disk Reads/sec", TotalInstance), f => f / 1024), Unit.Custom("kb/s"));
            context.Register("System Disk Reads/sec", () => new DerivedGauge(new PerformanceCounterGauge("PhysicalDisk", "Disk Writes/sec", TotalInstance), f => f / 1024), Unit.Custom("kb/s"));
        }

        public static void RegisterCLRGlobalCounters(MetricsContext context)
        {
            context.Register(".NET Mb in all Heaps", () => new DerivedGauge(new PerformanceCounterGauge(Memory, "# Bytes in all Heaps", GlobalInstance), v => v / (1024 * 1024)), Unit.Custom("Mb"));
            context.Register(".NET Time in GC", () => new PerformanceCounterGauge(Memory, "% Time in GC", GlobalInstance), Unit.Custom("%"));
        }

        public static void RegisterCLRAppCounters(MetricsContext context)
        {
            var app = Process.GetCurrentProcess().ProcessName;
            context.Register("Mb in all Heaps", () => new DerivedGauge(new PerformanceCounterGauge(Memory, "# Bytes in all Heaps", app), v => v / (1024 * 1024)), Unit.Custom("Mb"));
            context.Register("Time in GC", () => new PerformanceCounterGauge(Memory, "% Time in GC", app), Unit.Custom("%"));
            context.Register("Total Exceptions", () => new PerformanceCounterGauge(Exceptions, "# of Exceps Thrown", app), Unit.Custom("Exceptions"));
            context.Register("Exceptions Thrown / Sec", () => new PerformanceCounterGauge(Exceptions, "# of Exceps Thrown / Sec", app), Unit.Custom("Exceptions/s"));
            context.Register("Logical Threads", () => new PerformanceCounterGauge(LocksAndThreads, "# of current logical Threads", app), Unit.Custom("Threads"));
            context.Register("Physical Threads", () => new PerformanceCounterGauge(LocksAndThreads, "# of current physical Threads", app), Unit.Custom("Threads"));
            context.Register("Contention Rate / Sec", () => new PerformanceCounterGauge(LocksAndThreads, "Contention Rate / Sec", app), Unit.Custom("Attempts/s"));
            context.Register("Queue Length / sec", () => new PerformanceCounterGauge(LocksAndThreads, "Queue Length / sec", app), Unit.Custom("Threads/s"));

            ThreadPoolMetrics.RegisterThreadPoolGauges(context);
        }

        private static void Register(this MetricsContext context, string name, Func<MetricValueProvider<double>> gauge, Unit unit)
        {
            context.Advanced.Gauge(name, gauge, unit);
        }
    }
}
