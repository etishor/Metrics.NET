using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Metrics.Core
{
    public class MultiContextRegistry : MetricsRegistry
    {
        private readonly ConcurrentDictionary<string, MetricContext> contexts = new ConcurrentDictionary<string, MetricContext>();
        private readonly MetricsRegistry defaultRegistry;

        public MultiContextRegistry()
            : this(Process.GetCurrentProcess().ProcessName) { }

        public MultiContextRegistry(string name)
        {
            this.Name = name;
            this.defaultRegistry = Context(string.Empty).Registry;
            this.Config = new MetricsConfig().WithRegistry(this);
        }

        public string Name { get; private set; }

        public MetricsConfig Config { get; private set; }

        public MetricContext Context(string contextName)
        {
            return this.contexts.GetOrAdd(contextName, c => new MetricContext(c));
        }

        public void Shutdown(string contextName)
        {
            if (contextName == string.Empty)
            {
                throw new InvalidOperationException("The default context can't be shutdown");
            }

            MetricContext context;
            if (this.contexts.TryRemove(contextName, out context))
            {
                using (context) { }
            }
        }

        public IEnumerable<GaugeValueSource> Gauges { get { return this.contexts.SelectMany(c => c.Value.Registry.Gauges); } }
        public IEnumerable<CounterValueSource> Counters { get { return this.contexts.SelectMany(c => c.Value.Registry.Counters); } }
        public IEnumerable<MeterValueSource> Meters { get { return this.contexts.SelectMany(c => c.Value.Registry.Meters); } }
        public IEnumerable<HistogramValueSource> Histograms { get { return this.contexts.SelectMany(c => c.Value.Registry.Histograms); } }
        public IEnumerable<TimerValueSource> Timers { get { return this.contexts.SelectMany(c => c.Value.Registry.Timers); } }

        public Gauge Gauge(string name, Func<double> valueProvider, Unit unit)
        {
            return this.defaultRegistry.Gauge(name, valueProvider, unit);
        }

        public Gauge Gauge<T>(string name, Func<T> gauge, Unit unit) where T : GaugeMetric
        {
            return this.defaultRegistry.Gauge(name, gauge, unit);
        }

        public Counter Counter(string name, Unit unit)
        {
            return this.defaultRegistry.Counter(name, unit);
        }

        public Meter Meter(string name, Unit unit, TimeUnit rateUnit)
        {
            return this.defaultRegistry.Meter(name, unit, rateUnit);
        }

        public Histogram Histogram(string name, Unit unit, SamplingType samplingType)
        {
            return this.defaultRegistry.Histogram(name, unit, samplingType);
        }

        public Timer Timer(string name, Unit unit, SamplingType samplingType, TimeUnit rateUnit, TimeUnit durationUnit)
        {
            return this.defaultRegistry.Timer(name, unit, samplingType, rateUnit, durationUnit);
        }

        public void ClearAllMetrics()
        {
            foreach (var context in this.contexts)
            {
                context.Value.Registry.ClearAllMetrics();
            }
        }
    }
}
