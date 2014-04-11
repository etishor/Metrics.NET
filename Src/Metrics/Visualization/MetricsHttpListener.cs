
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
        private const string NotFoundResponse = "<!doctype html><html><body>Resource not found</body></html>";
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
                ProcessRequest(this.httpListener.GetContext());
            }
        }

        private static void ProcessRequest(HttpListenerContext context)
        {
            switch (context.Request.RawUrl)
            {
                case "/":
                    WriteFlotApp(context);
                    break;
                case "/json":
                    WriteJsonMetrics(context);
                    break;
                case "/text":
                    WriteTextMetrics(context);
                    break;
                default:
                    WriteNotFound(context);
                    break;
            }
        }

        private static void WriteNotFound(HttpListenerContext context)
        {
            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(NotFoundResponse);
            }
            context.Response.ContentType = "text/html";
            context.Response.StatusCode = 404;
            context.Response.StatusDescription = "NOT FOUND";
            context.Response.Close();
        }

        private static void WriteTextMetrics(HttpListenerContext context)
        {
            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(Metric.GetAsHumanReadable());
            }
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.Close();
        }

        private static void WriteJsonMetrics(HttpListenerContext context)
        {
            var json = new RegistrySerializer().ValuesAsJson(Metric.Registry);
            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(json);
            }
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.Close();
        }

        private static void WriteFlotApp(HttpListenerContext context)
        {
            var app = FlotWebApp.GetFlotApp(new Uri(context.Request.Url, "json"));
            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(app);
            }
            context.Response.ContentType = "text/html";
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.Close();
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
