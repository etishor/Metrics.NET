using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Metrics;
using Metrics.Core;

namespace Owin.Metrics.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class PostAndPutRequestSizeHistogramMiddleware
    {
        private readonly Histogram histogram;
        private AppFunc next;

        public PostAndPutRequestSizeHistogramMiddleware(MetricsRegistry registry, string metricName)
        {
            this.histogram = registry.Histogram(metricName, Unit.Bytes, SamplingType.FavourRecent);
        }

        public void Initialize(AppFunc next)
        {
            this.next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var httpMethod = environment["owin.RequestMethod"].ToString().ToUpper();

            if (httpMethod == "POST" || httpMethod == "PUT")
            {
                var headers = (IDictionary<string, string[]>)environment["owin.RequestHeaders"];
                if (headers != null && headers.ContainsKey("Content-Length"))
                {
                    histogram.Update(long.Parse(headers["Content-Length"].First()));
                }
            }

            await next(environment);
        }
    }
}
