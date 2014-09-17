
using System;
using System.Diagnostics;
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
                    if (Metric.Config.ErrorHandler != null)
                    {
                        Metric.Config.ErrorHandler(x);
                    }
                    else
                    {
                        Trace.Fail("Error executing Functional Gauge. You can handle this exception by setting a handler on Metric.Config.WithErrorHandler()", x.ToString());
                    }
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
                    if (Metric.Config.ErrorHandler != null)
                    {
                        Metric.Config.ErrorHandler(x);
                    }
                    else
                    {
                        Trace.Fail("Error executing Derived Gauge. You can handle this exception by setting a handler on Metric.Config.WithErrorHandler()", x.ToString());
                    }
                    return double.NaN;
                }
            }
        }
    }
}
