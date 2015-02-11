using Metrics;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Owin.Metrics.Middleware
{
    /// <summary>
    /// Owin middleware that counts the number of active requests.
    /// </summary>
    public class ActiveRequestCounterMiddleware : MetricMiddleware
    {
        private readonly Counter activeRequests;
        private Func<IDictionary<string, object>, Task> next;

        public ActiveRequestCounterMiddleware(MetricsContext context, string metricName, Regex[] ignorePatterns)
            : base(ignorePatterns)
        {
            this.activeRequests = context.Counter(metricName, Unit.Custom("ActiveRequests"));
        }

        public void Initialize(Func<IDictionary<string, object>, Task> next)
        {
            this.next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            if (PerformMetric(environment))
            {
                this.activeRequests.Increment();

                await this.next(environment);

                this.activeRequests.Decrement();
            }
            else
            {
                await this.next(environment);
            }
        }
    }
}
