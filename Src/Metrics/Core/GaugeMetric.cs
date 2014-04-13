
using System;
namespace Metrics.Core
{
    public abstract class GaugeMetric : Gauge, MetricValue<GaugeValue>
    {
        protected abstract string GetValue();
        public GaugeValue Value { get { return new GaugeValue(this.GetValue()); } }
    }

    public sealed class SimpleGauge : GaugeMetric
    {
        private readonly Func<string> valueProvider;

        public SimpleGauge(Func<string> valueProvider)
        {
            this.valueProvider = valueProvider;
        }

        protected override string GetValue()
        {
            return valueProvider();
        }
    }
}
