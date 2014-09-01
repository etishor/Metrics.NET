using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Metrics;

namespace Owin.Metrics.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class ErrorMeterMiddleware
    {
        private readonly Meter errorMeter;
        private AppFunc next;

        public ErrorMeterMiddleware(MetricContext context, string metricName)
        {
            this.errorMeter = context.Meter(metricName, Unit.Errors, TimeUnit.Seconds);
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
