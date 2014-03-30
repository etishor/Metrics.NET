using System;

namespace Metrics.Core
{
    public class GaugeMetric : Gauge
    {
        private readonly Func<string> valueProvider;

        public GaugeMetric(Func<string> valueProvider)
        {
            this.valueProvider = valueProvider;
        }

        public GaugeValue Value { get { return new GaugeValue(this.valueProvider()); } }
    }
}
