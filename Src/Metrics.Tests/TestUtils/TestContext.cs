using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Metrics.Core;

namespace Metrics.Tests.TestUtils
{
    public class TestContext : BaseMetricsContext
    {
        private TestContext(string contextName, TestRegistry registry)
            : base(contextName, registry)
        {
            this.Registry = registry;
        }

        public TestContext(string contextName, TestClock clock, TestScheduler scheduler)
            : this(contextName, new TestRegistry(contextName, clock, scheduler))
        {
            this.Clock = clock;
            this.Scheduler = scheduler;
        }

        private TestContext(string contextName, TestClock clock)
            : this(contextName, clock, new TestScheduler(clock))
        { }

        public TestContext()
            : this("TestContext", new TestClock())
        { }

        public TestClock Clock { get; private set; }
        public TestScheduler Scheduler { get; private set; }
        public TestRegistry Registry { get; private set; }

        public override MetricsContext Context(string contextName)
        {
            if (string.IsNullOrEmpty(contextName))
            {
                return this;
            }

            return base.Context(contextName, (name) => new TestContext(name, this.Clock, this.Scheduler));
        }

        public double GaugeValue(params string[] nameWithContext)
        {
            return ValueFor(GetDataFor(nameWithContext).Gauges, nameWithContext);
        }

        public long CounterValue(params string[] nameWithContext)
        {
            return ValueFor(GetDataFor(nameWithContext).Counters, nameWithContext);
        }

        public MeterValue MeterValue(params string[] nameWithContext)
        {
            return ValueFor(GetDataFor(nameWithContext).Meters, nameWithContext);
        }

        public HistogramValue HistogramValue(params string[] nameWithContext)
        {
            return ValueFor(GetDataFor(nameWithContext).Histograms, nameWithContext);
        }

        public TimerValue TimerValue(params string[] nameWithContext)
        {
            return ValueFor(GetDataFor(nameWithContext).Timers, nameWithContext);
        }

        private T ValueFor<T>(IEnumerable<MetricValueSource<T>> values, string[] nameWithContext) where T : struct
        {
            var value = values.Where(t => t.Name == nameWithContext.Last())
                .Select(t => t.Value);

            value.Should().HaveCount(1, "No metric found with name {0} in context {1}. Available names: {2}", nameWithContext.Last(),
                string.Join(".", nameWithContext.Take(nameWithContext.Length - 1)), string.Join(",", values.Select(v => v.Name)));

            return value.Single();
        }

        public MetricsData GetDataFor(params string[] nameWithContext)
        {
            return GetContextFor(nameWithContext).MetricsData;
        }

        public TestContext GetContextFor(params string[] nameWithContext)
        {
            if (nameWithContext.Length == 1)
            {
                return this;
            }

            return (this.Context(nameWithContext.First()) as TestContext).GetContextFor(nameWithContext.Skip(1).ToArray());
        }
    }
}
