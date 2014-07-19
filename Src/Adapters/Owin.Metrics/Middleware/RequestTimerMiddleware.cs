using Metrics;
using Metrics.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Owin.Metrics.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    public class RequestTimerMiddleware
    {
        private const string TimerItemsKey = "__Mertics.RequestTimer__";
        private readonly MetricsRegistry _registry;
        public string MetricsPrefix { get; set; }
        private AppFunc _next;

        public RequestTimerMiddleware(MetricsRegistry registry)
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
            var requestTimer = _registry.Timer(Name("Requests"), Unit.Requests, SamplingType.FavourRecent, TimeUnit.Seconds, TimeUnit.Milliseconds);
            environment[TimerItemsKey] = requestTimer.NewContext();

            await _next(environment);

            var timer = environment[TimerItemsKey];
            using (timer as IDisposable) { }
            environment.Remove(TimerItemsKey);
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
