using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Metrics;
using Metrics.Utils;

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

                var parameters = environment["route.Parameters"] as IDictionary<string, object>;
                var controllerName = environment["superscribe.webapi.controllername"] as string;

                if (!string.IsNullOrWhiteSpace(controllerName))
                {
                    metricName = environment["owin.RequestMethod"] + " " + controllerName;

                    if (parameters != null && parameters.Any())
                    {
                        metricName += "/" + string.Join("/", parameters.Keys.Select(k => "{" + k + "}"));
                    }
                }

                if (httpResponseStatusCode != (int)HttpStatusCode.NotFound)
                {
                    var startTime = (long)environment[RequestStartTimeKey];
                    var elapsed = Clock.Default.Nanoseconds - startTime;
                    this.context.Timer(metricName, Unit.Requests, SamplingType.FavourRecent, TimeUnit.Seconds, TimeUnit.Milliseconds).Record(elapsed, TimeUnit.Nanoseconds);
                }
            }
            else
            {
                await next(environment);
            }
        }
    }
}
