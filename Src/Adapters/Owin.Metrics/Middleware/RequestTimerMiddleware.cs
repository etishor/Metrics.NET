using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metrics;
using Metrics.Core;

namespace Owin.Metrics.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    public class RequestTimerMiddleware
    {
        private const string TimerItemsKey = "__Mertics.RequestTimer__";
        private readonly MetricsRegistry registry;
        private AppFunc next;

        public RequestTimerMiddleware(MetricsRegistry registry)
        {
            this.registry = registry;
            this.MetricsPrefix = "Owin";
        }

        public string MetricsPrefix { get; set; }

        public void Initialize(AppFunc next)
        {
            this.next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var requestTimer = registry.Timer(Name("Requests"), Unit.Requests, SamplingType.FavourRecent, TimeUnit.Seconds, TimeUnit.Milliseconds);
            environment[TimerItemsKey] = requestTimer.NewContext();

            await next(environment);

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
