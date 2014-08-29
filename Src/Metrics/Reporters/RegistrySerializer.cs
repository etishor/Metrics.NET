using System;
using System.Linq;
using Metrics.Json;
using Metrics.Utils;

namespace Metrics.Reporters
{
    public static class RegistrySerializer
    {
        public static string GetAsHumanReadable(MetricsData metricsData, Func<HealthStatus> healthStatus)
        {
            var report = new StringReporter();
            report.RunReport(metricsData, healthStatus);
            return report.Result;
        }

        public static string GetAsJson(MetricsData metricsData)
        {
            return new JsonFormatter()
                .AddTimestamp(Clock.Default)
                .AddObject(metricsData.Gauges)
                .AddObject(metricsData.Counters)
                .AddObject(metricsData.Meters)
                .AddObject(metricsData.Histograms)
                .AddObject(metricsData.Timers)
                .GetJson();
        }

        public static object GetForSerialization(MetricsData metricsData)
        {
            return new
            {
                Gauges = metricsData.Gauges.ToDictionary(g => g.Name, g => g.Value),
                Counters = metricsData.Counters.ToDictionary(c => c.Name, c => c.Value),
                Meters = metricsData.Meters.ToDictionary(m => m.Name, m => m.Value.Scale(m.RateUnit)),
                Histograms = metricsData.Histograms.ToDictionary(h => h.Name, h => h.Value),
                Timers = metricsData.Timers.ToDictionary(t => t.Name, t => t.Value.Scale(t.RateUnit, t.DurationUnit)),
                Units = new
                {
                    Gauges = metricsData.Gauges.ToDictionary(g => g.Name, g => g.Unit.Name),
                    Counters = metricsData.Counters.ToDictionary(c => c.Name, c => c.Unit.Name),
                    Meters = metricsData.Meters.ToDictionary(m => m.Name, m => string.Format("{0}/{1}", m.Unit.Name, m.RateUnit.Unit())),
                    Histograms = metricsData.Histograms.ToDictionary(h => h.Name, h => h.Unit.Name),
                    Timers = metricsData.Timers.ToDictionary(t => t.Name, t => new
                    {
                        Rate = string.Format("{0}/{1}", t.Unit.Name, t.RateUnit.Unit()),
                        Duration = t.DurationUnit.Unit()
                    })
                }
            };
        }
    }
}
