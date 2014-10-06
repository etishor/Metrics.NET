﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using Metrics.Sampling;

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

        public bool AttachContext(string contextName, MetricsContext context)
        {
            if (this.isDisabled)
            {
                return true;
            }

            if (string.IsNullOrEmpty(contextName))
            {
                throw new ArgumentException("Context name can't be null or empty for attached contexts");
            }
            var attached = this.childContexts.GetOrAdd(contextName, context);
            return object.ReferenceEquals(attached, context);
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

        public void PerformanceCounter(string name, string counterCategory, string counterName, string counterInstance, Unit unit, MetricTags tags)
        {
            this.Gauge(name, () => this.metricsBuilder.BuildePerformanceCounter(name, unit, counterCategory, counterName, counterInstance), unit, tags);
        }

        public void Gauge(string name, Func<double> valueProvider, Unit unit, MetricTags tags)
        {
            this.Gauge(name, () => this.metricsBuilder.BuildGauge(name, unit, valueProvider), unit, tags);
        }

        public void Gauge(string name, Func<MetricValueProvider<double>> valueProvider, Unit unit, MetricTags tags)
        {
            this.registry.Gauge(name, valueProvider, unit, tags);
        }

        public Counter Counter(string name, Unit unit, MetricTags tags)
        {
            return this.Counter(name, unit, () => this.metricsBuilder.BuildCounter(name, unit), tags);
        }

        public Counter Counter<T>(string name, Unit unit, Func<T> builder, MetricTags tags)
            where T : CounterImplementation
        {
            return this.registry.Counter(name, builder, unit, tags);
        }

        public Meter Meter(string name, Unit unit, TimeUnit rateUnit, MetricTags tags)
        {
            return this.Meter(name, unit, () => this.metricsBuilder.BuildMeter(name, unit, rateUnit), rateUnit, tags);
        }

        public Meter Meter<T>(string name, Unit unit, Func<T> builder, TimeUnit rateUnit, MetricTags tags)
           where T : MeterImplementation
        {
            return this.registry.Meter(name, builder, unit, rateUnit, tags);
        }

        public Histogram Histogram(string name, Unit unit, SamplingType samplingType, MetricTags tags)
        {
            return this.Histogram(name, unit, () => this.metricsBuilder.BuildHistogram(name, unit, samplingType), tags);
        }

        public Histogram Histogram<T>(string name, Unit unit, Func<T> builder, MetricTags tags)
            where T : HistogramImplementation
        {
            return this.registry.Histogram(name, builder, unit, tags);
        }

        public Histogram Histogram(string name, Unit unit, Func<Reservoir> builder, MetricTags tags)
        {
            return Histogram(name, unit, () => this.metricsBuilder.BuildHistogram(name, unit, builder()), tags);
        }

        public Timer Timer(string name, Unit unit, SamplingType samplingType, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags)
        {
            return this.registry.Timer(name, () => this.metricsBuilder.BuildTimer(name, unit, rateUnit, durationUnit, samplingType), unit, rateUnit, durationUnit, tags);
        }

        public Timer Timer<T>(string name, Unit unit, Func<T> builder, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags)
            where T : TimerImplementation
        {
            return this.registry.Timer(name, builder, unit, rateUnit, durationUnit, tags);
        }

        public Timer Timer(string name, Unit unit, Func<Histogram> builder, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags)
        {
            return this.Timer(name, unit, () => this.metricsBuilder.BuildTimer(name, unit, rateUnit, durationUnit, builder()), rateUnit, durationUnit, tags);
        }

        public Timer Timer(string name, Unit unit, Func<Reservoir> builder, TimeUnit rateUnit, TimeUnit durationUnit, MetricTags tags)
        {
            return this.Timer(name, unit, () => this.metricsBuilder.BuildTimer(name, unit, rateUnit, durationUnit, builder()), rateUnit, durationUnit, tags);
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
