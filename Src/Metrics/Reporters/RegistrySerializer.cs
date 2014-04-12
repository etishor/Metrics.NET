using System.Linq;
using Metrics.Utils;

namespace Metrics.Reporters
{
    public static class RegistrySerializer
    {
        public static object GetForSerialization(MetricsRegistry registry)
        {
            return new
            {
                Gauges = registry.Gauges.ToDictionary(g => g.Name, g => g.Value.Value),
                Counters = registry.Counters.ToDictionary(c => c.Name, c => c.Value),
                Meters = registry.Meters.ToDictionary(m => m.Name, m => m.Value.Scale(m.RateUnit)),
                Histograms = registry.Histograms.ToDictionary(h => h.Name, h => h.Value),
                Timers = registry.Timers.ToDictionary(t => t.Name, t => t.Value.Scale(t.RateUnit, t.DurationUnit)),
                Units = new
                {
                    Gauges = registry.Gauges.ToDictionary(g => g.Name, g => g.Unit.Name),
                    Counters = registry.Counters.ToDictionary(c => c.Name, c => c.Unit.Name),
                    Meters = registry.Meters.ToDictionary(m => m.Name, m => string.Format("{0}/{1}", m.Unit.Name, m.RateUnit.Unit())),
                    Histograms = registry.Histograms.ToDictionary(h => h.Name, h => h.Unit.Name),
                    Timers = registry.Timers.ToDictionary(t => t.Name, t => new
                    {
                        Rate = string.Format("{0}/{1}", t.Unit.Name, t.RateUnit.Unit()),
                        Duration = t.DurationUnit.Unit()
                    })
                }
            };
        }

        public static string ValuesAsJson(MetricsRegistry registry)
        {
            return new JsonFormatter()
                .AddObject(registry.Gauges)
                .AddObject(registry.Counters)
                .AddObject(registry.Meters)
                .AddObject(registry.Histograms)
                .AddObject(registry.Timers)
                .GetJson();
        }
    }
}
