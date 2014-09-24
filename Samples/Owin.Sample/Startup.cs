using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Description;
using Metrics;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin.Metrics;

namespace Owin.Sample
{
    public class Startup
    {
        private static IApiExplorer apiExplorer;

        public void Configuration(IAppBuilder app)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            app.UseCors(CorsOptions.AllowAll);

            var httpConfig = new HttpConfiguration();
            httpConfig.MapHttpAttributeRoutes();
            apiExplorer = httpConfig.Services.GetApiExplorer();
            httpConfig.EnsureInitialized();

            Metric.Config
                .WithAllCounters()
                .WithReporting(r => r.WithConsoleReport(TimeSpan.FromSeconds(30)))
                .WithOwin(middleware => app.Use(middleware), config => config
                    .WithRequestMetricsConfig(c => c.WithAllOwinMetrics(), new[]
                    {
                        new Regex("(?i)^sampleignore"),
                        new Regex("(?i)^metrics"),
                        new Regex("(?i)^health"), 
                        new Regex("(?i)^json")
                    }, metricNameResolver: WebApiMetricNameResolver)
                    .WithMetricsEndpoint()
                );

            app.UseWebApi(httpConfig);
        }

        public string WebApiMetricNameResolver(IDictionary<string, object> environment)
        {
            var request = new OwinRequest(environment);

            var description = apiExplorer.ApiDescriptions
                .FirstOrDefault(x =>
                {
                    var path = request.Uri.AbsolutePath.ToString(CultureInfo.InvariantCulture).TrimStart(new[] { '/' });
                    var routeTemplateSectionCount = x.Route.RouteTemplate.Split(new[] { '/' }).Count();
                    var pathSections = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    var pathSectionCount = pathSections.Count();
                    var actualPathWithoutRouteParams = pathSections
                        .Take(pathSectionCount - x.ParameterDescriptions.Count)
                        .Aggregate(string.Empty, (current, section) => current + ("/" + section)).TrimStart(new[] { '/' });

                    // Return the web api description with matching HttpMethod, the actual route without param matches that of the request,
                    // and the number of sections in the request's URL are the same the route template's.

                    if (x.HttpMethod.Method != request.Method || routeTemplateSectionCount != pathSectionCount)
                    {
                        return false;
                    }
                    
                    if(routeTemplateSectionCount == 1)
                    {
                        return x.Route.RouteTemplate.Equals(actualPathWithoutRouteParams, StringComparison.InvariantCultureIgnoreCase);
                    }
                    else
                    {
                        return x.Route.RouteTemplate.StartsWith(actualPathWithoutRouteParams, StringComparison.InvariantCultureIgnoreCase);
                    }
                });

            if (description == null) return string.Empty;

            return request.Method + " " + description.Route.RouteTemplate;
        }
    }
}
