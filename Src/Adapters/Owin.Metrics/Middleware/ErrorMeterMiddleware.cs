using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Metrics;
using Metrics.Core;

namespace Owin.Metrics.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ErrorMeterMiddleware
    {
        private readonly Meter errorMeter;
        private AppFunc next;

        public ErrorMeterMiddleware(MetricsRegistry registry, string metricName)
        {
            this.errorMeter = registry.Meter(metricName, Unit.Errors, TimeUnit.Seconds);
        }

        public void Initialize(AppFunc next)
        {
            this.next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await next(environment);

            var httpResponseStatusCode = int.Parse(environment["owin.ResponseStatusCode"].ToString());

            if (httpResponseStatusCode == (int)HttpStatusCode.InternalServerError)
            {
                errorMeter.Mark();
            }
        }
    }
}
