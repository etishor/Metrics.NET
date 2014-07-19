using Metrics;
using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin.Metrics;
using System;
using System.Web.Http;

[assembly: OwinStartup(typeof(Owin.Sample.Startup))]

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

            app.UseMetrics(metrics => metrics.WithAllCounters().WithReporting(r => r.WithConsoleReport(TimeSpan.FromSeconds(30))),
                owinMetrics => owinMetrics.RegisterAllMetrics());

            var httpConfig = new HttpConfiguration();
            httpConfig.MapHttpAttributeRoutes();
            app.UseWebApi(httpConfig);
        }
    }
}
