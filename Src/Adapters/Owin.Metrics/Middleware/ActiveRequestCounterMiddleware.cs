using Metrics;
using Metrics.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Owin.Metrics.Middleware
{
    public class ActiveRequestCounterMiddleware
    {
        private readonly MetricsRegistry _registry;
        public string MetricsPrefix { get; set; }
        private Func<IDictionary<string, object>, Task> _next;

        public ActiveRequestCounterMiddleware(MetricsRegistry registry)
        {
            _registry = registry;
            MetricsPrefix = "Owin";
        }

        public void Initialize(Func<IDictionary<string, object>, Task> next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var counter = _registry.Counter(Name("ActiveRequests"), Unit.Custom("ActiveRequests"));
            counter.Increment();

            await _next(environment);

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
