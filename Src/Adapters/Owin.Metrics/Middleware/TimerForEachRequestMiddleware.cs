using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Metrics;
using Metrics.Core;
using Metrics.Utils;

namespace Owin.Metrics.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    public class TimerForEachRequestMiddleware
    {
        private const string RequestStartTimeKey = "__Metrics.RequestStartTime__";

        private readonly MetricsRegistry registry;
        private readonly string metricPrefix;
        
        private AppFunc next;

        public TimerForEachRequestMiddleware(MetricsRegistry registry, string metricPrefix)
        {
            this.registry = registry;
            this.metricPrefix = metricPrefix;
        }

        public void Initialize(AppFunc next)
        {
            this.next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            environment[RequestStartTimeKey] = Clock.Default.Nanoseconds;

            await next(environment);

            var httpResponseStatusCode = int.Parse(environment["owin.ResponseStatusCode"].ToString());
            var httpMethod = environment["owin.RequestMethod"].ToString().ToUpper();

            if (httpResponseStatusCode != (int)HttpStatusCode.NotFound)
            {
                var httpRequestPath = environment["owin.RequestPath"].ToString().ToUpper();
                var name = string.Format("{0}.{1} [{2}]", metricPrefix, httpMethod, httpRequestPath);
                var startTime = (long)environment[RequestStartTimeKey];
                var elapsed = Clock.Default.Nanoseconds - startTime;
                Metric.Timer(name, Unit.Requests).Record(elapsed, TimeUnit.Nanoseconds);
            }
        }
    }
}
