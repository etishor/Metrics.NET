using Microsoft.Owin;
using Owin;
using Owin.Metrics;

[assembly: OwinStartup(typeof(Metrics.AspSample.AppStartup))]

namespace Metrics.AspSample
{
    public class AppStartup
    {
        public void Configuration(IAppBuilder app)
        {
            Metric.Config
                .WithOwin(m => app.Use(m), c => c.WithMetricsEndpoint());

            app.UseWelcomePage();

            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
        }
    }
}
