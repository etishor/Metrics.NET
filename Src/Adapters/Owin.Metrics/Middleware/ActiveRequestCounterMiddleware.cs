using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metrics;
using Metrics.Core;

namespace Owin.Metrics.Middleware
{
    /// <summary>
    /// Owin middleware that counts the number of active requests.
    /// </summary>
    public class ActiveRequestCounterMiddleware
    {
        private readonly Counter activeRequests;
        private Func<IDictionary<string, object>, Task> next;

        public ActiveRequestCounterMiddleware(MetricsRegistry registry, string metricName)
        {
            this.activeRequests = registry.Counter(metricName, Unit.Custom("ActiveRequests"));
        }

        public void Initialize(Func<IDictionary<string, object>, Task> next)
        {
            this.next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            this.activeRequests.Increment();

            await this.next(environment);

            this.activeRequests.Decrement();
        }
    }
}
