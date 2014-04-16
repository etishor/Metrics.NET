
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
                try
                {
                    ProcessRequest(this.httpListener.GetContext());
                }
                catch (HttpListenerException ex)
                {
                    if (ex.ErrorCode != 995) // IO operation aborted
                    {
                        throw;
                    }
                }
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
                case "/ping":
                    WritePong(context);
                    break;
                case "/health":
                    WriteHealthStatus(context);
                    break;
                default:
                    WriteNotFound(context);
                    break;
            }
        }

        private static void WriteHealthStatus(HttpListenerContext context)
        {
            var status = HealthChecks.GetStatus();
            var json = HealthCheckSerializer.Serialize(status);

            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
            context.Response.ContentType = "application/json";

            context.Response.StatusCode = status.IsHealty ? 200 : 500;
            context.Response.StatusDescription = status.IsHealty ? "OK" : "Internal Server Error";

            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(json);
            }
            context.Response.Close();
        }

        private static void WritePong(HttpListenerContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write("pong");
            }
            context.Response.Close();
        }

        private static void WriteNotFound(HttpListenerContext context)
        {
            context.Response.ContentType = "text/html";
            context.Response.StatusCode = 404;
            context.Response.StatusDescription = "NOT FOUND";
            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(NotFoundResponse);
            }
            context.Response.Close();
        }

        private static void WriteTextMetrics(HttpListenerContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(RegistrySerializer.GetAsHumanReadable(Metric.Registry, HealthChecks.Registry));
            }
            context.Response.Close();
        }

        private static void WriteJsonMetrics(HttpListenerContext context)
        {
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            var json = RegistrySerializer.GetAsJson(Metric.Registry);
            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(json);
            }
            context.Response.Close();
        }

        private static void WriteFlotApp(HttpListenerContext context)
        {
            context.Response.ContentType = "text/html";
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            var app = FlotWebApp.GetFlotApp(new Uri(context.Request.Url, "json"));
            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(app);
            }
            context.Response.Close();
        }

        public void Stop()
        {
            cts.Cancel();
            this.httpListener.Stop();
        }

        public void Dispose()
        {
            this.Stop();
            this.httpListener.Close();
            using (this.cts) { }
            using (this.httpListener) { }
        }
    }
}
