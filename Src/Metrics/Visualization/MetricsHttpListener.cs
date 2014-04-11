
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Metrics.Reporters;
namespace Metrics.Visualization
{
    public sealed class MetricsHttpListener : IDisposable
    {
        private readonly HttpListener httpListener;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public MetricsHttpListener(string listenerUriPrefix)
        {
            this.httpListener = new HttpListener();
            this.httpListener.Prefixes.Add(listenerUriPrefix);
        }

        public void Start()
        {
            this.httpListener.Start();
            Task.Factory.StartNew(ProcessRequests, TaskCreationOptions.LongRunning);
        }

        private void ProcessRequests()
        {
            while (!this.cts.IsCancellationRequested)
            {
                var context = this.httpListener.GetContext();

                if (context.Request.Url.ToString().EndsWith("/json"))
                {
                    var json = new RegistrySerializer().ValuesAsJson(Metric.Registry);
                    using (var writer = new StreamWriter(context.Response.OutputStream))
                    {
                        context.Response.ContentType = "application/json";
                        writer.Write(json);
                    }
                }
                else
                {
                    var app = FlotWebApp.GetFlotApp(new Uri(context.Request.Url, "json"));
                    using (var writer = new StreamWriter(context.Response.OutputStream))
                    {
                        writer.Write(app);
                    }
                }
            }
        }

        public void Stop()
        {
            cts.Cancel();
            this.httpListener.Stop();
        }

        public void Dispose()
        {
            this.httpListener.Close();
            using (this.httpListener) { }
        }
    }
}
