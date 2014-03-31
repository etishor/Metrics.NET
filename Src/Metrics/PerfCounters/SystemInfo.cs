
using System;
namespace Metrics.PerfCounters
{
    public class SystemInfo : BaseCounterRegristry
    {
        public SystemInfo(MetricsRegistry registry, string prefix)
            : base(registry, prefix) { }

        public void Register()
        {
            Register("MachineName", () => Environment.MachineName);
            Register("Processor Count", () => Environment.ProcessorCount.ToString());
            Register("OperatingSystem", () => GetOSVersion());
            Register("NETVersion", () => Environment.Version.ToString());
            Register("CommandLine", () => Environment.CommandLine);
            Register("AvailableRAM", new PerfCounter("Memory", "Available MBytes"), Unit.Custom("Mb"));
            Register("CPU Usage", new PerfCounter("Processor", "% Processor Time", TotalInstance), Unit.Custom("%"));
        }

        private string GetOSVersion()
        {
            return string.Format("{0} {1}", Environment.OSVersion.VersionString, Environment.Is64BitOperatingSystem ? "64bit" : "32bit");
        }
    }
}
