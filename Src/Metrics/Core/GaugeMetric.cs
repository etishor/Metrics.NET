
using System;
namespace Metrics.Core
{
    public sealed class FunctionGauge : MetricValueProvider<double>
    {
        private readonly Func<double> valueProvider;

        public FunctionGauge(Func<double> valueProvider)
        {
            this.valueProvider = valueProvider;
        }

        public double Value { get { return this.valueProvider(); } }
    }

    public sealed class DerivedGauge : MetricValueProvider<double>
    {
        private readonly MetricValueProvider<double> gauge;
        private readonly Func<double, double> transformation;

        public DerivedGauge(MetricValueProvider<double> gauge, Func<double, double> transformation)
        {
            this.gauge = gauge;
            this.transformation = transformation;
        }

        public double Value { get { return this.transformation(this.gauge.Value); } }
    }
}
