using System;
using System.Linq;
using Metrics.Core;
namespace Metrics.Tests.TestUtils
{
    public class TestContext : BaseMetricsContext
    {
        private readonly TestRegistry registry;

        private TestContext(string contextName, TestRegistry registry)
            : base(contextName, registry)
        {
            this.registry = registry;
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

        public override MetricsContext Context(string contextName)
        {
            if (string.IsNullOrEmpty(contextName))
            {
                return this;
            }

            return base.Context(contextName, (name) => new TestContext(name, this.Clock, this.Scheduler));
        }

        public void ForAllTimers(Action<TimerValue> test)
        {
            foreach (var timer in this.MetricsData.Timers)
            {
                test(timer.Value);
            }
        }

        public TimerValue TimerValue(string contextDottedName)
        {
            int index = contextDottedName.IndexOf('.');

            if (index > 0)
            {
                var context = contextDottedName.Substring(0, index);
                var remaining = contextDottedName.Substring(index + 1);
                return (this.Context(context) as TestContext).TimerValue(remaining);
            }

            return this.MetricsData.Timers
                .Where(t => t.Name == contextDottedName)
                .Select(t => t.Value)
                .Single();
        }
    }
}
