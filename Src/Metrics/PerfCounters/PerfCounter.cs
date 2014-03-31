using System;
using System.Diagnostics;

namespace Metrics.PerfCounters
{
    public struct PerfCounter
    {
        private readonly string category;
        private readonly string counter;
        private readonly string instance;

        public PerfCounter(string category, string counter)
            : this(category, counter, null)
        { }

        public PerfCounter(string category, string counter, string instance)
            : this()
        {
            this.category = category;
            this.counter = counter;
            this.instance = instance;
            Format = v => v.ToString("F");
        }

        public Func<float, string> Format { get; set; }

        public string GetValue()
        {
            return this.Format(GetCounterValue());
        }

        private float GetCounterValue()
        {
            using (var perfCounter = new PerformanceCounter(category, counter, instance, true))
            {
                return perfCounter.NextValue();
            }
        }
    }
}
