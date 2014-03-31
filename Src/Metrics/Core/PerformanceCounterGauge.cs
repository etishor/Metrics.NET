using System.Diagnostics;

namespace Metrics.Core
{
    public class PerformanceCounterGauge : GaugeMetric
    {
        public const string GlobalInstance = "_Global_";

        public PerformanceCounterGauge(string category, string counter)
            : this(category, counter, null)
        { }

        public PerformanceCounterGauge(string category, string counter, string instance)
            : base(() => GetValue(category, counter, instance))
        {

        }

        private static string GetValue(string category, string counter, string instance)
        {
            using (var perfCounter = new PerformanceCounter(category, counter, instance, true))
            {
                return perfCounter.NextValue().ToString("F");
            }
        }
    }
}
