using Metrics;
using Metrics.Core;
using Metrics.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Owin.Metrics.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    public class TimerForEachRequestMiddleware
    {
        private const string RequestStartTimeKey = "__Metrics.RequestStartTime__";
        private readonly MetricsRegistry _registry;
        public string MetricsPrefix { get; set; }
        private AppFunc _next;

        public TimerForEachRequestMiddleware(MetricsRegistry registry)
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
            environment[RequestStartTimeKey] = Clock.Default.Nanoseconds;

            await _next(environment);

            var httpResponseStatusCode = int.Parse(environment["owin.ResponseStatusCode"].ToString());
            var httpMethod = environment["owin.RequestMethod"].ToString().ToUpper();

            if (httpResponseStatusCode != (int)HttpStatusCode.NotFound)
            {
                var httpRequestPath = environment["owin.RequestPath"].ToString().ToUpper();
                var name = string.Format("{0}.{1} [{2}]", MetricsPrefix, httpMethod, httpRequestPath);
                var startTime = (long)environment[RequestStartTimeKey];
                var elapsed = Clock.Default.Nanoseconds - startTime;
                Metric.Timer(name, Unit.Requests).Record(elapsed, TimeUnit.Nanoseconds);
            }

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
