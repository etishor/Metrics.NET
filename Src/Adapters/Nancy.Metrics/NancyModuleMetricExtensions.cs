using System;
using Metrics;
using Nancy.Routing;

namespace Nancy.Metrics
{
    public static class NancyModuleMetricExtensions
    {
        private static Predicate<RouteDescription> MakePredicate(this INancyModule module, string methodName, string pathPrefix)
        {
            if (string.IsNullOrEmpty(pathPrefix) || !pathPrefix.StartsWith("/"))
            {
                throw new ArgumentException("pathPrefix must start with / ", pathPrefix);
            }

            var path = (module.ModulePath + pathPrefix).ToUpper();

            return d => (string.IsNullOrEmpty(methodName) || methodName.ToUpper() == "ANY" || d.Method.ToUpper() == methodName.ToUpper())
                && d.Path.ToUpper().StartsWith(path);
        }

        public static void MetricForRequestTime(this INancyModule module, string metricName, Predicate<RouteDescription> routePredicate)
        {
            var name = string.Format("{0}.{1}", module.GetType().Name, metricName);
            var timer = NancyMetrics.CurrentConfig.Registry.Timer(name, Unit.Requests, SamplingType.FavourRecent, TimeUnit.Seconds, TimeUnit.Milliseconds);
            var key ="Metrics.Nancy.Request.Timer." + metricName;

            module.Before.AddItemToStartOfPipeline(ctx =>
            {
                if (routePredicate(ctx.ResolvedRoute.Description))
                {
                    ctx.Items[key] = timer.NewContext();
                }
                return null;
            });

            module.After.AddItemToEndOfPipeline(ctx =>
            {
                if (routePredicate(ctx.ResolvedRoute.Description))
                {
                    using (ctx.Items[key] as IDisposable) { }
                    ctx.Items.Remove(key);
                }
            });
        }

        public static void MetricForRequestTime(this INancyModule module, string metricName, string method, string pathPrefix)
        {
            module.MetricForRequestTime(metricName, module.MakePredicate(method, pathPrefix));
        }

        public static void MetricForRequestTime<T>(this INancyModule module, string metricName, string method, string pathPrefix)
            where T : INancyModule
        {
            module.MetricForRequestTime(typeof(T).Name + "." + metricName, module.MakePredicate(method, pathPrefix));
        }
    }
}
