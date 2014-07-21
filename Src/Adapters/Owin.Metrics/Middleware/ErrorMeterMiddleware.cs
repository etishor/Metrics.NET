using Metrics;
using Metrics.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Owin.Metrics.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    public class ErrorMeterMiddleware
    {
        private readonly MetricsRegistry _registry;
        public string MetricsPrefix { get; set; }
        private AppFunc _next;

        public ErrorMeterMiddleware(MetricsRegistry registry)
        {
            _registry = registry;
            MetricsPrefix = "Owin";
        }

        public void Initialize(AppFunc next)
        {
            _next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await _next(environment);

            var httpResponseStatusCode = int.Parse(environment["owin.ResponseStatusCode"].ToString());
            var errorMeter = _registry.Meter(Name("Errors"), Unit.Errors, TimeUnit.Seconds);
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
