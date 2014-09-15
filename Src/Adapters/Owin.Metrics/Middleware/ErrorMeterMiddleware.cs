using Metrics;
using Metrics.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Owin.Metrics.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ErrorMeterMiddleware : MetricMiddleware
    {
        private readonly Meter errorMeter;
        private AppFunc next;

        public ErrorMeterMiddleware(MetricsRegistry registry, string metricName, Regex[] ignorePatterns)
            : base(ignorePatterns)
        {
            this.errorMeter = registry.Meter(metricName, Unit.Errors, TimeUnit.Seconds);
        }

        public void Initialize(AppFunc next)
        {
            this.next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            if (PerformMetric(environment))
            {
                await next(environment);

                var httpResponseStatusCode = int.Parse(environment["owin.ResponseStatusCode"].ToString());

                if (httpResponseStatusCode == (int)HttpStatusCode.InternalServerError)
                {
                    errorMeter.Mark();
                }
            }
            else
            {
                await next(environment);
            }
        }
    }
}
