using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metrics;
using Metrics.Core;

namespace Owin.Metrics.Middleware
{
    public class ActiveRequestCounterMiddleware
    {
        private readonly MetricsRegistry registry;
        private Func<IDictionary<string, object>, Task> next;

        public ActiveRequestCounterMiddleware(MetricsRegistry registry)
        {
            this.registry = registry;
            this.MetricsPrefix = "Owin";
        }

        public string MetricsPrefix { get; set; }

        public void Initialize(Func<IDictionary<string, object>, Task> next)
        {
            this.next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var counter = registry.Counter(Name("ActiveRequests"), Unit.Custom("ActiveRequests"));
            counter.Increment();

            await next(environment);

            counter.Decrement();
        }

        private string Name(string name)
        {
            if (!string.IsNullOrEmpty(MetricsPrefix))
            {
                return MetricsPrefix + "." + name;
            }
            return name;
        }
    }
}
