
using System;
using System.Collections.Concurrent;
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

        public bool Merge(MetricValueProvider<double> other)
        {
            valueProviders.Add(() => other.Value);
            return true;
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
        
        public bool Merge(MetricValueProvider<double> other)
        {
            return gauge.Merge(other);
        }
    }

    public abstract class RatioGauge : GaugeImplementation
    {

        /// <summary>
        /// A ratio of one quantity to another.
        /// </summary>
        public sealed class Ratio 
        {
            /// <summary>
            /// Creates a new ratio with the given numerator and denominator.
            /// </summary>
            /// <param name="numerator">the numerator of the ratio</param>
            /// <param name="denominator">the denominator of the ratio</param>
            /// <returns>numerator:denominator</returns>
            public static Ratio Of(double numerator, double denominator) 
            {
                return new Ratio(numerator, denominator);
            }

            private readonly double _numerator;
            private readonly double _denominator;

            private Ratio(double numerator, double denominator) 
            {
                _numerator = numerator;
                _denominator = denominator;
            }

            /// <summary>
            /// Returns the ratio, which is either a {@code double} between 0 and 1 (inclusive) or Double.NaN.
            /// </summary>
            public double Value {
                get
                {
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (double.IsNaN(_denominator) || double.IsInfinity(_denominator) || _denominator == 0)
                    {
                        return Double.NaN;
                    }
                    return _numerator / _denominator;
                }
            }

            public override string ToString()
            {
                return _numerator + ":" + _denominator;
            }

        }

        /// <summary>
        /// Returns the {@link Ratio} which is the gauge's current value.
        /// </summary>
        /// <returns>the {@link Ratio} which is the gauge's current value</returns>
        protected abstract Ratio GetRatio();

        public double Value
        {
            get { return GetRatio().Value; }
        }

        public double GetValue(bool resetMetric = false)
        {
            return Value;
        }

        public abstract bool Merge(MetricValueProvider<double> other);
    //{
    //    throw new NotImplementedException();
    //}
    }

    public sealed class MeterRatioGauge : RatioGauge
    {

        private readonly ConcurrentBag<Func<double>> _hitValueFuncs = new ConcurrentBag<Func<double>>();
        private readonly ConcurrentBag<Func<double>> _totalValueFuncs = new ConcurrentBag<Func<double>>();
 
        /// <summary>
        /// Creates a new MeterRatioGauge with externally tracked Meters, and uses the OneMinuteRate from the MeterValue of the meters.
        /// </summary>
        /// <param name="hitMeter"></param>
        /// <param name="totalMeter"></param>
        public MeterRatioGauge(MetricValueSource<MeterValue> hitMeter, MetricValueSource<MeterValue> totalMeter)
            : this(hitMeter, totalMeter, value => value.OneMinuteRate)
        {
            
        }

        public MeterRatioGauge(MetricValueSource<MeterValue> hitMeter, MetricValueSource<MeterValue> totalMeter, Func<MeterValue, double> meterRateFunc)
        {
            _hitValueFuncs.Add(() => meterRateFunc(hitMeter.Value));
            _totalValueFuncs.Add(() => meterRateFunc(totalMeter.Value));
        }

        public MeterRatioGauge(MetricValueSource<MeterValue> hitMeter, TimerValueSource totalTimer)
            : this(hitMeter, totalTimer, value => value.OneMinuteRate)
        {
        }

        public MeterRatioGauge(MetricValueSource<MeterValue> hitMeter, TimerValueSource totalTimer, Func<MeterValue, double> meterRateFunc)
        {
            _hitValueFuncs.Add(() => meterRateFunc(hitMeter.Value));
            _totalValueFuncs.Add(() => meterRateFunc(totalTimer.Value.Rate));
        }

        protected override Ratio GetRatio()
        {
            return Ratio.Of(_hitValueFuncs.Sum(f =>f()),_totalValueFuncs.Sum(f=>f()));
        }

        public override bool Merge(MetricValueProvider<double> other)
        {
            var otherMeterGauge = other as MeterRatioGauge;
            if (otherMeterGauge == null)
                return false;

            foreach (var f in otherMeterGauge._hitValueFuncs)
                _hitValueFuncs.Add(f);

            foreach (var f in _totalValueFuncs)
                _totalValueFuncs.Add(f);

            return true;
        }
    }
}
