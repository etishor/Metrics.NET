
using System;
using System.Collections.Generic;
using System.Linq;
using Metrics.MetricData;
namespace Metrics.Core
{
    public interface GaugeImplementation : MetricValueProvider<double> { }

    public sealed class FunctionGauge : GaugeImplementation
    {
        private readonly List<Func<double>> valueProviders;

        public FunctionGauge(Func<double> valueProvider)
        {
            this.valueProviders = new List<Func<double>>(new[] { valueProvider });
        }

        public double GetValue(bool resetMetric = false)
        {
            return Value;
        }

        public double Value
        {
            get
            {
                try
                {
                    if (valueProviders.Count > 1)
                    {
                        var vals = valueProviders.AsParallel().Select(vp => vp()).ToArray();
                        Array.Sort(vals);

                        // get the median gauge value
                        return (vals[(vals.Length-1)/2]);
                    }
                    return valueProviders[0]();
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
            return Value;
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
