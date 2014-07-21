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
        private readonly MetricsRegistry registry;
        private AppFunc next;

        public ErrorMeterMiddleware(MetricsRegistry registry)
        {
            this.registry = registry;
            MetricsPrefix = "Owin";
        }

        public string MetricsPrefix { get; set; }

        public void Initialize(AppFunc next)
        {
            this.next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await next(environment);

            var httpResponseStatusCode = int.Parse(environment["owin.ResponseStatusCode"].ToString());
            var errorMeter = registry.Meter(Name("Errors"), Unit.Errors, TimeUnit.Seconds);
            if (httpResponseStatusCode == (int)HttpStatusCode.InternalServerError)
            {
                errorMeter.Mark();
            }
        }

        private string Name(string name)
        {
            if (!string.IsNullOrEmpty(MetricsPrefix))
            {
                return MetricsPrefix + "." + name;
            }
            return name;
        }
    }
}
