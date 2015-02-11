
using System;
using Metrics.MetricData;
namespace Metrics.Core
{
    public interface GaugeImplementation : MetricValueProvider<double> { }

    public sealed class FunctionGauge : GaugeImplementation
    {
        private readonly Func<double> valueProvider;

        public FunctionGauge(Func<double> valueProvider)
        {
            this.valueProvider = valueProvider;
        }

        public double GetValue(bool resetMetric = false)
        {
            return this.Value;
        }

        public double Value
        {
            get
            {
                try
                {
                    return this.valueProvider();
                }
                catch (Exception x)
                {
                    MetricsErrorHandler.Handle(x, "Error executing Functional Gauge");
                    return double.NaN;
                }
            }
        }
    }

    public sealed class DerivedGauge : GaugeImplementation
    {
        private readonly MetricValueProvider<double> gauge;
        private readonly Func<double, double> transformation;

        public DerivedGauge(MetricValueProvider<double> gauge, Func<double, double> transformation)
        {
            this.gauge = gauge;
            this.transformation = transformation;
        }

        public double GetValue(bool resetMetric = false)
        {
            return this.Value;
        }

        public double Value
        {
            get
            {
                try
                {
                    return this.transformation(this.gauge.Value);
                }
                catch (Exception x)
                {
                    MetricsErrorHandler.Handle(x, "Error executing Derived Gauge");
                    return double.NaN;
                }
            }
        }
    }
}
