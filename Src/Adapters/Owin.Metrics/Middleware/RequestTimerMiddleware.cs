using Metrics;
using Metrics.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Owin.Metrics.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class RequestTimerMiddleware : MetricMiddleware
    {
        private const string TimerItemsKey = "__Mertics.RequestTimer__";

        private readonly Timer requestTimer;
        private AppFunc next;

        public RequestTimerMiddleware(MetricsRegistry registry, string metricName, Regex[] ignorePatterns)
            : base(ignorePatterns)
        {
            this.requestTimer = registry.Timer(metricName, Unit.Requests, SamplingType.FavourRecent, TimeUnit.Seconds, TimeUnit.Milliseconds);
        }

        public void Initialize(AppFunc next)
        {
            this.next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            if (base.PerformMetric(environment))
            {
                environment[TimerItemsKey] = this.requestTimer.NewContext();

                await next(environment);

                var timer = environment[TimerItemsKey];
                using (timer as IDisposable)
                {
                }
                environment.Remove(TimerItemsKey);
            }
            else
            {
                await next(environment);
            }

        }
    }
}
