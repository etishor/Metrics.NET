
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Metrics.Core;
using Metrics.Reporters;
namespace Metrics.Visualization
{
    public sealed class MetricsHttpListener : IDisposable
    {
        private const string NotFoundResponse = "<!doctype html><html><body>Resource not found</body></html>";
        private readonly HttpListener httpListener;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly MetricsRegistry registry;
        private readonly Func<HealthStatus> healthStatus;

        public MetricsHttpListener(string listenerUriPrefix, MetricsRegistry registry, Func<HealthStatus> healthStatus)
        {
            this.httpListener = new HttpListener();
            this.httpListener.Prefixes.Add(listenerUriPrefix);
            this.registry = registry;
            this.healthStatus = healthStatus;
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
                    // ReSharper disable EmptyGeneralCatchClause
                catch (Exception)
                    // ReSharper restore EmptyGeneralCatchClause
                {
                    // Keep listener running, regardless of errors.
                }
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            switch (context.Request.RawUrl)
            {
                case "/":
                    if (context.Request.Url.ToString().EndsWith("/"))
                    {
                        WriteFlotApp(context);
                    }
                    else
                    {
                        context.Response.Redirect(context.Request.Url.ToString() + "/");
                        context.Response.Close();
                    }
                    break;
                case "/json":
                    WriteJsonMetrics(context, this.registry);
                    break;
                case "/text":
                    WriteTextMetrics(context, this.registry, this.healthStatus);
                    break;
                case "/ping":
                    WritePong(context);
                    break;
                case "/health":
                    WriteHealthStatus(context, this.healthStatus);
                    break;
                default:
                    WriteNotFound(context);
                    break;
            }
        }

        private static void AddNoCacheHeaders(HttpListenerResponse response)
        {
            response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            response.Headers.Add("Pragma", "no-cache");
            response.Headers.Add("Expires", "0");
        }

        private static void WriteHealthStatus(HttpListenerContext context, Func<HealthStatus> healthStatus)
        {
            var status = healthStatus();
            var json = HealthCheckSerializer.Serialize(status);

            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");

            AddNoCacheHeaders(context.Response);

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

            AddNoCacheHeaders(context.Response);

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

        private static void WriteTextMetrics(HttpListenerContext context, MetricsRegistry registry, Func<HealthStatus> healthStatus)
        {
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";

            AddNoCacheHeaders(context.Response);

            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(RegistrySerializer.GetAsHumanReadable(registry, healthStatus));
            }
            context.Response.Close();
        }

        private static void WriteJsonMetrics(HttpListenerContext context, MetricsRegistry registry)
        {
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";

            AddNoCacheHeaders(context.Response);

            var json = RegistrySerializer.GetAsJson(registry);
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
            var app = FlotWebApp.GetFlotApp();
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
