using System;
using System.Diagnostics;

namespace Metrics.PerfCounters
{
    public class PerformanceCounterGauge : Gauge
    {
        private static readonly Func<float, string> DefaultFormat = f => f.ToString("F");

        private readonly string category;
        private readonly string counter;
        private readonly string instance;
        private readonly Func<float, string> format;
        private readonly PerformanceCounter performanceCounter;

        public PerformanceCounterGauge(string category, string counter)
            : this(category, counter, instance: null)
        { }

        public PerformanceCounterGauge(string category, string counter, Func<float, string> format)
            : this(category, counter, instance: null, format: format)
        { }

        public PerformanceCounterGauge(string category, string counter, string instance)
            : this(category, counter, instance, DefaultFormat)
        { }

        public PerformanceCounterGauge(string category, string counter, string instance, Func<float, string> format)
        {
            this.category = category;
            this.counter = counter;
            this.instance = instance;
            this.format = format;
            this.performanceCounter = new PerformanceCounter(category, counter, instance, true);
        }

        public GaugeValue Value
        {
            get
            {
                return new GaugeValue(format(this.performanceCounter.NextValue()));
            }
        }
    }
}
