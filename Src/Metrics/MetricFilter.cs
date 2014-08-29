
using System;
using System.Collections.Generic;
namespace Metrics
{
    public enum MetricType
    {
        Gauge,
        Counter,
        Meter,
        Histogram,
        Timer
    }

    public interface MetricFilter
    {
        bool IsMatch(GaugeValueSource gauge);
        bool IsMatch(CounterValueSource counter);
        bool IsMatch(MeterValueSource meter);
        bool IsMatch(HistogramValueSource histogram);
        bool IsMatch(TimerValueSource timer);
    }

    public class Filter : MetricFilter
    {
        private Predicate<string> context = null;
        private Predicate<string> name = null;
        private HashSet<MetricType> types = null;

        private class NoOpFilter : MetricFilter
        {
            public bool IsMatch(GaugeValueSource gauge) { return true; }
            public bool IsMatch(CounterValueSource counter) { return true; }
            public bool IsMatch(MeterValueSource meter) { return true; }
            public bool IsMatch(HistogramValueSource histogram) { return true; }
            public bool IsMatch(TimerValueSource timer) { return true; }
        }

        public static MetricFilter All = new NoOpFilter();

        private Filter() { }

        public static Filter New()
        {
            return new Filter();
        }

        public Filter WhereContext(Predicate<string> condition)
        {
            this.context = condition;
            return this;
        }

        public Filter WhereContext(string context)
        {
            return WhereContext(c => c.Equals(context, StringComparison.InvariantCultureIgnoreCase));
        }

        public Filter WhereName(Predicate<string> condition)
        {
            this.name = condition;
            return this;
        }

        public Filter WhereNameStartsWith(string name)
        {
            return WhereName(n => n.StartsWith(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public Filter WhereType(params MetricType[] types)
        {
            this.types = new HashSet<MetricType>(types);
            return this;
        }

        public bool IsMatch(GaugeValueSource gauge)
        {
            if (types != null && !types.Contains(MetricType.Gauge))
            {
                return false;
            }
            return IsMatch(gauge.Context, gauge.Name);
        }

        public bool IsMatch(CounterValueSource counter)
        {
            if (types != null && !types.Contains(MetricType.Counter))
            {
                return false;
            }
            return IsMatch(counter.Context, counter.Name);
        }

        public bool IsMatch(MeterValueSource meter)
        {
            if (types != null && !types.Contains(MetricType.Meter))
            {
                return false;
            }
            return IsMatch(meter.Context, meter.Name);
        }

        public bool IsMatch(HistogramValueSource histogram)
        {
            if (types != null && !types.Contains(MetricType.Histogram))
            {
                return false;
            }
            return IsMatch(histogram.Context, histogram.Name);
        }

        public bool IsMatch(TimerValueSource timer)
        {
            if (types != null && !types.Contains(MetricType.Timer))
            {
                return false;
            }
            return IsMatch(timer.Context, timer.Name);
        }

        private bool IsMatch(string context, string name)
        {
            if (this.context != null && !this.context(context))
            {
                return false;
            }

            if (this.name != null && !this.name(name))
            {
                return false;
            }

            return true;
        }
    }
}
