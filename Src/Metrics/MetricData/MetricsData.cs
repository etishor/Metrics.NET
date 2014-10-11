
using System.Collections.Generic;
using System.Linq;

namespace Metrics.MetricData
{
    public sealed class MetricsData
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
                .Select(g => new GaugeValueSource(FormatName(prefix, g.Name), g.ValueProvider, g.Unit, g.Tags))
                .Union(this.ChildMetrics.SelectMany(m => m.OldFormat(FormatPrefix(prefix, m.Context)).Gauges));

            var counters = this.Counters
                .Select(c => new CounterValueSource(FormatName(prefix, c.Name), c.ValueProvider, c.Unit, c.Tags))
                .Union(this.ChildMetrics.SelectMany(m => m.OldFormat(FormatPrefix(prefix, m.Context)).Counters));

            var meters = this.Meters
                .Select(m => new MeterValueSource(FormatName(prefix, m.Name), m.ValueProvider, m.Unit, m.RateUnit, m.Tags))
                .Union(this.ChildMetrics.SelectMany(m => m.OldFormat(FormatPrefix(prefix, m.Context)).Meters));

            var histograms = this.Histograms
                .Select(h => new HistogramValueSource(FormatName(prefix, h.Name), h.ValueProvider, h.Unit, h.Tags))
                .Union(this.ChildMetrics.SelectMany(m => m.OldFormat(FormatPrefix(prefix, m.Context)).Histograms));

            var timers = this.Timers
                .Select(t => new TimerValueSource(FormatName(prefix, t.Name), t.ValueProvider, t.Unit, t.RateUnit, t.DurationUnit, t.Tags))
                .Union(this.ChildMetrics.SelectMany(m => m.OldFormat(FormatPrefix(prefix, m.Context)).Timers));

            return new MetricsData(this.Context, gauges, counters, meters, histograms, timers);
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
