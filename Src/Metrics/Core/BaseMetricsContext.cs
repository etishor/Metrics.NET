using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Metrics.Core
{
    public abstract class BaseMetricsContext : MetricsContext, AdvancedMetricsContext
    {
        private readonly ConcurrentDictionary<string, MetricsContext> childContexts = new ConcurrentDictionary<string, MetricsContext>();

        private readonly string context;
        private MetricsRegistry registry;
        private MetricsBuilder metricsBuilder;

        private bool isDisabled;

        public BaseMetricsContext(string context, MetricsRegistry registry, MetricsBuilder metricsBuilder)
        {
            this.context = context;
            this.registry = registry;
            this.metricsBuilder = metricsBuilder;
            this.DataProvider = new DefaultDataProvider(this.context, this.registry.DataProvider, () => this.childContexts.Values.Select(c => c.DataProvider));
        }

        protected abstract MetricsContext CreateChildContextInstance(string contextName);

        public AdvancedMetricsContext Advanced { get { return this; } }

        public event EventHandler ContextShuttingDown;
        public event EventHandler ContextDisabled;

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
            this.Gauge(name, () => this.metricsBuilder.BuildePerformanceCounter(name, unit, counterCategory, counterName, counterInstance), unit);
        }

        public void Gauge(string name, Func<double> valueProvider, Unit unit)
        {
            this.Gauge(name, () => this.metricsBuilder.BuildGauge(name, unit, valueProvider), unit);
        }

        public void Gauge(string name, Func<MetricValueProvider<double>> valueProvider, Unit unit)
        {
            this.registry.Gauge(name, valueProvider, unit);
        }

        public Counter Counter(string name, Unit unit)
        {
            return this.Counter(name, unit, () => this.metricsBuilder.BuildCounter(name, unit));
        }

        public Counter Counter<T>(string name, Unit unit, Func<T> builder)
            where T : Counter, MetricValueProvider<long>
        {
            return this.registry.Counter(name, unit, builder);
        }

        public Meter Meter(string name, Unit unit, TimeUnit rateUnit = TimeUnit.Seconds)
        {
            return this.Meter(name, unit, () => this.metricsBuilder.BuildMeter(name, unit, rateUnit), rateUnit);
        }

        public Meter Meter<T>(string name, Unit unit, Func<T> builder, TimeUnit rateUnit = TimeUnit.Seconds)
           where T : Meter, MetricValueProvider<MeterValue>
        {
            return this.registry.Meter(name, unit, rateUnit, builder);
        }

        public Histogram Histogram(string name, Unit unit, SamplingType samplingType = SamplingType.FavourRecent)
        {
            return this.Histogram(name, unit, () => this.metricsBuilder.BuildHistogram(name, unit, samplingType));
        }

        public Histogram Histogram<T>(string name, Unit unit, Func<T> builder)
            where T : Histogram, MetricValueProvider<HistogramValue>
        {
            return this.registry.Histogram(name, unit, builder);
        }

        public Histogram Histogram(string name, Unit unit, Func<Reservoir> builder)
        {
            return Histogram(name, unit, () => this.metricsBuilder.BuildHistogram(name, unit, builder()));
        }

        public Timer Timer(string name, Unit unit, SamplingType samplingType = SamplingType.FavourRecent,
            TimeUnit rateUnit = TimeUnit.Seconds, TimeUnit durationUnit = TimeUnit.Milliseconds)
        {
            return this.registry.Timer(name, unit, rateUnit, durationUnit, () => this.metricsBuilder.BuildTimer(name, unit, rateUnit, durationUnit, samplingType));
        }

        public Timer Timer<T>(string name, Unit unit, Func<T> builder, TimeUnit rateUnit = TimeUnit.Seconds, TimeUnit durationUnit = TimeUnit.Milliseconds)
            where T : Timer, MetricValueProvider<TimerValue>
        {
            return this.registry.Timer(name, unit, rateUnit, durationUnit, builder);
        }

        public Timer Timer(string name, Unit unit, Func<Histogram> builder, TimeUnit rateUnit = TimeUnit.Seconds, TimeUnit durationUnit = TimeUnit.Milliseconds)
        {
            return this.Timer(name, unit, () => this.metricsBuilder.BuildTimer(name, unit, rateUnit, durationUnit, builder()), rateUnit, durationUnit);
        }

        public Timer Timer(string name, Unit unit, Func<Reservoir> builder, TimeUnit rateUnit = TimeUnit.Seconds, TimeUnit durationUnit = TimeUnit.Milliseconds)
        {
            return this.Timer(name, unit, () => this.metricsBuilder.BuildTimer(name, unit, rateUnit, durationUnit, builder()), rateUnit, durationUnit);
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
            using (oldRegistry as IDisposable) { }

            ForAllChildContexts(c => c.Advanced.CompletelyDisableMetrics());

            if (this.ContextShuttingDown != null)
            {
                this.ContextShuttingDown(this, EventArgs.Empty);
            }

            if (this.ContextDisabled != null)
            {
                this.ContextDisabled(this, EventArgs.Empty);
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

        public void ResetMetricsValues()
        {
            this.registry.ResetMetricsValues();
            ForAllChildContexts(c => c.Advanced.ResetMetricsValues());
        }

        public void WithCustomMetricsBuilder(MetricsBuilder metricsBuilder)
        {
            this.metricsBuilder = metricsBuilder;
            ForAllChildContexts(c => c.Advanced.WithCustomMetricsBuilder(metricsBuilder));
        }

        private void ForAllChildContexts(Action<MetricsContext> action)
        {
            foreach (var context in this.childContexts.Values)
            {
                context.Advanced.ResetMetricsValues();
            }
        }

    }
}
