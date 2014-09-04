
using System.Collections.Generic;
using System.Linq;
namespace Metrics
{
    public struct MetricsData
    {
        public static MetricsData Empty = new MetricsData(string.Empty,
            Enumerable.Empty<GaugeValueSource>(),
            Enumerable.Empty<CounterValueSource>(),
            Enumerable.Empty<MeterValueSource>(),
            Enumerable.Empty<HistogramValueSource>(),
            Enumerable.Empty<TimerValueSource>());

        public readonly string Context;

        public readonly IEnumerable<GaugeValueSource> Gauges;
        public readonly IEnumerable<CounterValueSource> Counters;
        public readonly IEnumerable<MeterValueSource> Meters;
        public readonly IEnumerable<HistogramValueSource> Histograms;
        public readonly IEnumerable<TimerValueSource> Timers;
        public readonly IEnumerable<MetricsData> ChildMetrics;

        public MetricsData(string context, MetricsData data, IEnumerable<MetricsData> childMetrics)
            : this(context, data.Gauges, data.Counters, data.Meters, data.Histograms, data.Timers, childMetrics)
        { }

        public MetricsData(string context, IEnumerable<GaugeValueSource> gauges, IEnumerable<CounterValueSource> counters,
            IEnumerable<MeterValueSource> meters, IEnumerable<HistogramValueSource> histograms, IEnumerable<TimerValueSource> timers)
            : this(context, gauges, counters, meters, histograms, timers, Enumerable.Empty<MetricsData>())
        { }

        public MetricsData(string context, IEnumerable<GaugeValueSource> gauges, IEnumerable<CounterValueSource> counters,
            IEnumerable<MeterValueSource> meters, IEnumerable<HistogramValueSource> histograms, IEnumerable<TimerValueSource> timers,
            IEnumerable<MetricsData> childMetrics)
        {
            this.Context = context;
            this.Gauges = gauges;
            this.Counters = counters;
            this.Meters = meters;
            this.Histograms = histograms;
            this.Timers = timers;
            this.ChildMetrics = childMetrics;
        }

        public MetricsData Filter(MetricsFilter filter)
        {
            if (!filter.IsMatch(this.Context))
            {
                return MetricsData.Empty;
            }

            return new MetricsData(this.Context,
                this.Gauges.Where(g => filter.IsMatch(g)),
                this.Counters.Where(c => filter.IsMatch(c)),
                this.Meters.Where(m => filter.IsMatch(m)),
                this.Histograms.Where(h => filter.IsMatch(h)),
                this.Timers.Where(t => filter.IsMatch(t)),
                this.ChildMetrics.Select(m => m.Filter(filter)));
        }

        public MetricsData Flaten()
        {
            return new MetricsData(this.Context,
                this.Gauges.Union(this.ChildMetrics.SelectMany(m => m.Flaten().Gauges)),
                this.Counters.Union(this.ChildMetrics.SelectMany(m => m.Flaten().Counters)),
                this.Meters.Union(this.ChildMetrics.SelectMany(m => m.Flaten().Meters)),
                this.Histograms.Union(this.ChildMetrics.SelectMany(m => m.Flaten().Histograms)),
                this.Timers.Union(this.ChildMetrics.SelectMany(m => m.Flaten().Timers))
            );
        }

        public MetricsData OldFormat()
        {
            return OldFormat(string.Empty);
        }

        private MetricsData OldFormat(string prefix)
        {
            var gauges = this.Gauges
                .Select(g => new GaugeValueSource(FormatName(prefix, g.Name), Constant(g.Value), g.Unit))
                .Union(this.ChildMetrics.SelectMany(m => m.OldFormat(FormatPrefix(prefix, m.Context)).Gauges));

            var counters = this.Counters
                .Select(c => new CounterValueSource(FormatName(prefix, c.Name), Constant(c.Value), c.Unit))
                .Union(this.ChildMetrics.SelectMany(m => m.OldFormat(FormatPrefix(prefix, m.Context)).Counters));

            var meters = this.Meters
                .Select(m => new MeterValueSource(FormatName(prefix, m.Name), Constant(m.Value), m.Unit, m.RateUnit))
                .Union(this.ChildMetrics.SelectMany(m => m.OldFormat(FormatPrefix(prefix, m.Context)).Meters));

            var histograms = this.Histograms
                .Select(h => new HistogramValueSource(FormatName(prefix, h.Name), Constant(h.Value), h.Unit))
                .Union(this.ChildMetrics.SelectMany(m => m.OldFormat(FormatPrefix(prefix, m.Context)).Histograms));

            var timers = this.Timers
                .Select(t => new TimerValueSource(FormatName(prefix, t.Name), Constant(t.Value), t.Unit, t.RateUnit, t.DurationUnit))
                .Union(this.ChildMetrics.SelectMany(m => m.OldFormat(FormatPrefix(prefix, m.Context)).Timers));

            return new MetricsData(this.Context, gauges, counters, meters, histograms, timers);
        }

        private class Provider<T> : MetricValueProvider<T> where T : struct
        {
            public Provider(T value)
            {
                this.Value = value;
            }
            public T Value { get; private set; }
        }

        private static MetricValueProvider<T> Constant<T>(T value) where T : struct
        {
            return new Provider<T>(value);
        }

        private static string FormatPrefix(string prefix, string context)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return context;
            }
            return prefix + " - " + context;
        }

        private static string FormatName(string prefix, string name)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                return name;
            }
            return string.Format("[{0}] {1}", prefix, name);
        }
    }
}
