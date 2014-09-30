using Metrics;
using Microsoft.Owin.Cors;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin.Metrics;
using Superscribe.Models;
using Superscribe.Owin.Engine;
using Superscribe.Owin.Extensions;
using Superscribe.WebApi;
using Superscribe.WebApi.Owin.Extensions;
using System;
using System.Text.RegularExpressions;
using System.Web.Http;
using String = Superscribe.Models.String;

namespace Owin.Sample
{
    public class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            app.UseCors(CorsOptions.AllowAll);


            var engine = OwinRouteEngineFactory.Create();

            var httpconfig = new HttpConfiguration();
            SuperscribeConfig.Register(httpconfig, engine);

            engine.Route("sample".Controller());
            engine.Route(r => r / "sample".Controller() / (Int)"x" / (String)"y");

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
                     })
                    .WithMetricsEndpoint()
                );

            app.UseSuperscribeRouter(engine)
                .UseWebApi(httpconfig)
                .WithSuperscribe(httpconfig, engine);
        }

    }
}
