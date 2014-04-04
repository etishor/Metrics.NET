using Nancy;
using Nancy.Bootstrapper;
using Nancy.Metrics;
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

            NancyMetrics.Configure()
                .WithGlobalMetrics(config => config.RegisterAllMetrics(pipelines))
                .WithMetricsEndpoint();

            pipelines.AfterRequest += ctx =>
            {
                if (ctx.Response != null)
                {
                    ctx.Response
                        .WithHeader("Access-Control-Allow-Origin", "*")
                        .WithHeader("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
                }
            };
            // to enable authentication use .WithMetricsEndpoint( "/stats", m => m.RequiresAuthentication() ) 
        }

        protected override void ConfigureApplicationContainer(TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register(typeof(JsonSerializer), typeof(CustomJsonSerializer));
        }
    }
}
