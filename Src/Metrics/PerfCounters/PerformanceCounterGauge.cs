using System.Diagnostics;
using Metrics.Core;

namespace Metrics.PerfCounters
{
    public class PerformanceCounterGauge : GaugeMetric
    {
        private readonly PerformanceCounter performanceCounter;

        public PerformanceCounterGauge(string category, string counter)
            : this(category, counter, instance: null)
        { }

        public PerformanceCounterGauge(string category, string counter, string instance)
        {
            this.performanceCounter = instance == null ?
                new PerformanceCounter(category, counter, true) :
                new PerformanceCounter(category, counter, instance, true);
        }

        public override double Value { get { return this.performanceCounter.NextValue(); } }
    }
}
