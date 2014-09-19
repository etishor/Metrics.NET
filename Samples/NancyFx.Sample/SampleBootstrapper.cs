using System;
using System.Collections.Generic;
using Metrics;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Bootstrapper;
using Nancy.Security;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NancyFx.Sample
{
    public class SampleBootstrapper : DefaultNancyBootstrapper
    {
        internal class CustomJsonSerializer : JsonSerializer
        {
            public CustomJsonSerializer()
            {
                this.ContractResolver = new CamelCasePropertyNamesContractResolver();
                this.Formatting = Formatting.Indented;
            }
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            StatelessAuthentication.Enable(pipelines, new StatelessAuthenticationConfiguration(AuthenticateUser));

            Metric.Config
                .WithAllCounters()
                .WithReporting(r => r.WithConsoleReport(TimeSpan.FromSeconds(30)))
                .WithNancy(pipelines, config => config
                    .WithNancyMetrics(c => c.WithAllMetrics())
                    .WithMetricsModule(m => m.RequiresAuthentication(), "/admin/metrics")
                );

            pipelines.AfterRequest += ctx =>
            {
                if (ctx.Response != null)
                {
                    ctx.Response
                        .WithHeader("Access-Control-Allow-Origin", "*")
                        .WithHeader("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
                }
            };
        }

        class FakeUser : IUserIdentity
        {
            public IEnumerable<string> Claims { get { yield return "Admin"; } }
            public string UserName
            {
                get { return "admin"; }
            }
        }

        private IUserIdentity AuthenticateUser(NancyContext context)
        {
            return new FakeUser();
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register(typeof(JsonSerializer), typeof(CustomJsonSerializer));
        }
    }
}
