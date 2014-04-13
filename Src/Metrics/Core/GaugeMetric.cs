
using System;
namespace Metrics.Core
{
    public abstract class GaugeMetric : Gauge, MetricValueProvider<double>
    {
        public abstract double Value { get; }
    }

    public sealed class FunctionGauge : GaugeMetric
    {
        private readonly Func<double> valueProvider;

        public FunctionGauge(Func<double> valueProvider)
        {
            this.valueProvider = valueProvider;
        }

        public override double Value { get { return this.valueProvider(); } }
    }

    public sealed class DerivedGauge : GaugeMetric
    {
        private readonly GaugeMetric gauge;
        private readonly Func<double, double> transformation;

        public DerivedGauge(GaugeMetric gauge, Func<double, double> transformation)
        {
            this.gauge = gauge;
            this.transformation = transformation;
        }

        public override double Value { get { return this.transformation(this.gauge.Value); } }
    }
}
