using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Metrics;

namespace Owin.Metrics.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class RequestTimerMiddleware : MetricMiddleware
    {
        private const string RequestStartTimeKey = "__Mertics.RequestStartTime__";

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
                environment[RequestStartTimeKey] = this.requestTimer.StartRecording();

                await next(environment);

                var endTime = this.requestTimer.EndRecording();
                var startTime = (long)environment[RequestStartTimeKey];
                this.requestTimer.Record(endTime - startTime, TimeUnit.Nanoseconds);

                environment.Remove(RequestStartTimeKey);
            }
            else
            {
                await next(environment);
            }

        }
    }
}
