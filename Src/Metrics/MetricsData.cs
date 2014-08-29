
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
    }
}
