using Topshelf;

namespace Metrics.Central
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<MetricsService>();

                x.StartAutomatically()
                 .RunAsLocalService();

                x.SetDescription("Metrics.NET Central Service");
                x.SetDisplayName("Metrics.NET Central");
                x.SetServiceName("Metrics.Central");
            });
        }
    }
}
