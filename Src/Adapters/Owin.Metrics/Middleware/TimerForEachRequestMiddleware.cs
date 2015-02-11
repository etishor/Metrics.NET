using Metrics;
using Metrics.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Owin.Metrics.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class TimerForEachRequestMiddleware : MetricMiddleware
    {
        private const string RequestStartTimeKey = "__Metrics.RequestStartTime__";

        private readonly MetricsContext context;

        private AppFunc next;

        public TimerForEachRequestMiddleware(MetricsContext context, Regex[] ignorePatterns)
            : base(ignorePatterns)
        {
            this.context = context;
        }

        public void Initialize(AppFunc next)
        {
            this.next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            if (PerformMetric(environment))
            {
                environment[RequestStartTimeKey] = Clock.Default.Nanoseconds;

                await next(environment);

                var httpResponseStatusCode = int.Parse(environment["owin.ResponseStatusCode"].ToString());
                var metricName = environment["owin.RequestPath"].ToString();

                if (environment.ContainsKey("metrics-net.routetemplate"))
                {
                    var requestMethod = environment["owin.RequestMethod"] as string;
                    var routeTemplate = environment["metrics-net.routetemplate"] as string;

                    metricName = requestMethod.ToUpperInvariant() + " " + routeTemplate;
                }

                if (httpResponseStatusCode != (int)HttpStatusCode.NotFound)
                {
                    var startTime = (long)environment[RequestStartTimeKey];
                    var elapsed = Clock.Default.Nanoseconds - startTime;
                    this.context.Timer(metricName, Unit.Requests)
                        .Record(elapsed, TimeUnit.Nanoseconds);
                }
            }
            else
            {
                await next(environment);
            }
        }
    }
}
