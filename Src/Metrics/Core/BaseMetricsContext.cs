using System;
using System.Collections.Concurrent;
using System.Linq;
using Metrics.PerfCounters;

namespace Metrics.Core
{
    public abstract class BaseMetricsContext : MetricsContext
    {
        private readonly ConcurrentDictionary<string, MetricsContext> childContexts = new ConcurrentDictionary<string, MetricsContext>();

        private readonly string context;
        private MetricsRegistry registry;

        private bool isDisabled;

        public BaseMetricsContext(string context, MetricsRegistry registry)
        {
            this.context = context;
            this.registry = registry;

            this.DataProvider = new DefaultDataProvider(this.context, this.registry.DataProvider, () => this.childContexts.Values.Select(c => c.DataProvider));
        }

        protected abstract MetricsContext CreateChildContextInstance(string contextName);

        public event EventHandler ContextShuttingDown;
        public MetricsDataProvider DataProvider { get; private set; }

        public MetricsContext Context(string contextName)
        {
            return this.Context(contextName, c => CreateChildContextInstance(contextName));
        }

        public MetricsContext Context(string contextName, Func<string, MetricsContext> contextCreator)
        {
            if (this.isDisabled)
            {
                return this;
            }

            if (string.IsNullOrEmpty(contextName))
            {
                return this;
            }

            return this.childContexts.GetOrAdd(contextName, contextCreator);
        }

        public void ShutdownContext(string contextName)
        {
            if (string.IsNullOrEmpty(contextName))
            {
                throw new ArgumentException("contextName must not be null or empty", contextName);
            }

            MetricsContext context;
            if (this.childContexts.TryRemove(contextName, out context))
            {
                using (context) { }
            }
        }

        public void PerformanceCounter(string name, string counterCategory, string counterName, string counterInstance, Unit unit)
        {
            this.Gauge(name, () => new PerformanceCounterGauge(counterCategory, counterName, counterInstance), unit);
        }

        public void Gauge(string name, Func<double> valueProvider, Unit unit)
        {
            this.Gauge(name, () => new FunctionGauge(valueProvider), unit);
        }

        public void Gauge(string name, Func<MetricValueProvider<double>> valueProvider, Unit unit)
        {
            this.registry.Gauge(name, valueProvider, unit);
        }

        public Meter Meter(string name, Unit unit, TimeUnit rateUnit = TimeUnit.Seconds)
        {
            return this.registry.Meter(name, unit, rateUnit);
        }

        public Counter Counter(string name, Unit unit)
        {
            return this.registry.Counter(name, unit);
        }

        public Histogram Histogram(string name, Unit unit, SamplingType samplingType = SamplingType.FavourRecent)
        {
            return this.registry.Histogram(name, unit, samplingType);
        }

        public Timer Timer(string name, Unit unit, SamplingType samplingType = SamplingType.FavourRecent,
            TimeUnit rateUnit = TimeUnit.Seconds, TimeUnit durationUnit = TimeUnit.Milliseconds)
        {
            return this.registry.Timer(name, unit, samplingType, rateUnit, durationUnit);
        }

        public void CompletelyDisableMetrics()
        {
            if (this.isDisabled)
            {
                return;
            }

            this.isDisabled = true;

            var oldRegistry = this.registry;
            this.registry = new NullMetricsRegistry();
            oldRegistry.ClearAllMetrics();
            foreach (var context in this.childContexts.Values)
            {
                context.CompletelyDisableMetrics();
            }

            if (this.ContextShuttingDown != null)
            {
                this.ContextShuttingDown(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            if (!this.isDisabled)
            {
                if (this.ContextShuttingDown != null)
                {
                    this.ContextShuttingDown(this, EventArgs.Empty);
                }
            }
        }
    }
}
