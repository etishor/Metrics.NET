using Metrics;
using Metrics.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Owin.Metrics.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    public class PostAndPutRequestSizeHistogramMiddleware
    {
        private readonly MetricsRegistry _registry;
        public string MetricsPrefix { get; set; }
        private AppFunc _next;

        public PostAndPutRequestSizeHistogramMiddleware(MetricsRegistry registry)
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
            // start post and put request size
            var histogram = _registry.Histogram(Name("PostAndPutRequestsSize"), Unit.Custom("bytes"), SamplingType.FavourRecent);
            var httpMethod = environment["owin.RequestMethod"].ToString().ToUpper();
            if (httpMethod == "POST" || httpMethod == "PUT")
            {
                var headers = (IDictionary<string, string[]>)environment["owin.RequestHeaders"];
                if (headers != null && headers.ContainsKey("Content-Length"))
                    histogram.Update(long.Parse(headers["Content-Length"].First()));
            }

            await _next(environment);
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
