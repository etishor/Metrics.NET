using Metrics;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Owin.Metrics.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class RequestTimerMiddleware : MetricMiddleware
    {
        private const string TimerItemsKey = "__Mertics.RequestTimer__";

        private readonly Timer requestTimer;
        private AppFunc next;

        public RequestTimerMiddleware(MetricsContext context, string metricName, Regex[] ignorePatterns)
            : base(ignorePatterns)
        {
            this.requestTimer = context.Timer(metricName, Unit.Requests);
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
