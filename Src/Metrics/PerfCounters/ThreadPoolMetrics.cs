using System;
using System.Diagnostics;
using System.Threading;

namespace Metrics.PerfCounters
{
    public static class ThreadPoolMetrics
    {
        public static void RegisterThreadPoolGauges(MetricsContext context)
        {
            context.Gauge("Thread Pool Available Threads", () => { int threads, ports; ThreadPool.GetAvailableThreads(out threads, out ports); return threads; }, Unit.Threads);
            context.Gauge("Thread Pool Available Completion Ports", () => { int threads, ports; ThreadPool.GetAvailableThreads(out threads, out ports); return ports; }, Unit.Custom("Ports"));

            context.Gauge("Thread Pool Min Threads", () => { int threads, ports; ThreadPool.GetMinThreads(out threads, out ports); return threads; }, Unit.Threads);
            context.Gauge("Thread Pool Min Completion Ports", () => { int threads, ports; ThreadPool.GetMinThreads(out threads, out ports); return ports; }, Unit.Custom("Ports"));

            context.Gauge("Thread Pool Max Threads", () => { int threads, ports; ThreadPool.GetMaxThreads(out threads, out ports); return threads; }, Unit.Threads);
            context.Gauge("Thread Pool Max Completion Ports", () => { int threads, ports; ThreadPool.GetMaxThreads(out threads, out ports); return ports; }, Unit.Custom("Ports"));

            var currentProcess = Process.GetCurrentProcess();
            context.Gauge(currentProcess.ProcessName + " Uptime", () => (DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalSeconds, Unit.Custom("Seconds"));
            context.Gauge(currentProcess.ProcessName + " Threads", () => Process.GetCurrentProcess().Threads.Count, Unit.Threads);
        }
    }
}
