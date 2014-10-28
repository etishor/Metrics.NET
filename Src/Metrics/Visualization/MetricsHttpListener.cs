
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Metrics.Json;
using Metrics.MetricData;
using Metrics.Reporters;
namespace Metrics.Visualization
{
    public sealed class MetricsHttpListener : IDisposable
    {
        private const string NotFoundResponse = "<!doctype html><html><body>Resource not found</body></html>";
        private readonly HttpListener httpListener;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly MetricsDataProvider metricsDataProvider;
        private readonly Func<HealthStatus> healthStatus;
        private readonly string prefixPath;

        private static readonly Timer timer = Metric.Internal.Timer("HTTP Request", Unit.Requests);
        private static readonly Meter errors = Metric.Internal.Meter("HTTP Request Errors", Unit.Errors);
        private static readonly Histogram jsonSize = Metric.Internal.Histogram("HTTP JSON Size", Unit.KiloBytes);

        public MetricsHttpListener(string listenerUriPrefix, MetricsDataProvider metricsDataProvider, Func<HealthStatus> healthStatus)
        {
            this.prefixPath = ParsePrefixPath(listenerUriPrefix);
            this.httpListener = new HttpListener();
            this.httpListener.Prefixes.Add(listenerUriPrefix);
            this.metricsDataProvider = metricsDataProvider;
            this.healthStatus = healthStatus;
        }

        private string ParsePrefixPath(string listenerUriPrefix)
        {
            var match = Regex.Match(listenerUriPrefix, @"http://(?:[^/]*)(?:\:\d+)?/(.*)");
            if (match.Success)
            {
                return match.Groups[1].Value.ToLowerInvariant();
            }
            else
            {
                return string.Empty;
            }
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
                    HttpListenerContext context = null;
                    try
                    {
                        context = this.httpListener.GetContext();
                        using (timer.NewContext())
                        {
                            ProcessRequest(context);
                            context.Response.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Mark();
                        context.Response.StatusCode = 500;
                        context.Response.StatusDescription = "Internal Server Error";
                        context.Response.Close();
                        MetricsErrorHandler.Handle(ex, "Error processing HTTP request");
                    }
                }
                catch (Exception ex)
                {
                    errors.Mark();
                    HttpListenerException httpException = ex as HttpListenerException;
                    if (httpException == null || httpException.ErrorCode != 995)// IO operation aborted
                    {
                        MetricsErrorHandler.Handle(ex, "Error processing HTTP request");
                    }
                }
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            if (context.Request.HttpMethod.ToUpperInvariant() != "GET")
            {
                return WriteNotFound(context);
            }

            var urlPath = context.Request.RawUrl.Substring(this.prefixPath.Length)
                .ToLowerInvariant();

            switch (urlPath)
            {
                case "/":
                    if (!context.Request.Url.ToString().EndsWith("/"))
                    {
                        context.Response.Redirect(context.Request.Url.ToString() + "/");
                        context.Response.Close();
                    }
                    else
                    {
                        WriteFlotApp(context);
                    }
                    break;
                case "/favicon.ico":
                    WriteFavIcon(context);
                    break;
                case "/json":
                    WriteJsonMetrics(context, this.metricsDataProvider);
                    break;
                case "/v1/json":
                    WriteJsonMetricsV1(context, this.metricsDataProvider);
                    break;
                case "/v2/json":
                    WriteJsonMetricsV2(context, this.metricsDataProvider);
                    break;
                case "/health":
                    WriteHealthStatus(context, this.healthStatus);
                    break;
                case "/v1/health":
                    WriteHealthStatus(context, this.healthStatus);
                    break;
                case "/text":
                    WriteTextMetrics(context, this.metricsDataProvider, this.healthStatus);
                    break;
                case "/ping":
                    WritePong(context);
                    break;
                default:
                    WriteNotFound(context);
                    break;

            }
        }

        private static void WriteHealthStatus(HttpListenerContext context, Func<HealthStatus> healthStatus)
        {
            var status = healthStatus();
            var json = JsonHealthChecks.BuildJson(status);

            WriteString(context, json, JsonHealthChecks.HealthChecksMimeType);
            context.Response.StatusCode = status.IsHealty ? 200 : 500;
            context.Response.StatusDescription = status.IsHealty ? "OK" : "Internal Server Error";
        }

        private static void WritePong(HttpListenerContext context)
        {
            WriteString(context, "pong", "text/plain");
        }

        private static void WriteNotFound(HttpListenerContext context)
        {
            WriteString(context, NotFoundResponse, "text/plain");
            context.Response.StatusCode = 404;
            context.Response.StatusDescription = "NOT FOUND";
        }

        private void WriteTextMetrics(HttpListenerContext context, MetricsDataProvider metricsDataProvider, Func<HealthStatus> healthStatus)
        {
            var text = StringReporter.RenderMetrics(metricsDataProvider.CurrentMetricsData, healthStatus);
            WriteString(context, text, "text/plain");
        }

        private static void WriteJsonMetrics(HttpListenerContext context, MetricsDataProvider metricsDataProvider)
        {
            var acceptHeader = context.Request.Headers["Accept"] ?? string.Empty;

            if (acceptHeader.Contains(JsonBuilderV2.MetricsMimeType))
            {
                WriteJsonMetricsV2(context, metricsDataProvider);
            }
            else
            {
                WriteJsonMetricsV1(context, metricsDataProvider);
            }
        }

        private static void WriteJsonMetricsV1(HttpListenerContext context, MetricsDataProvider metricsDataProvider)
        {
            var json = JsonBuilderV1.BuildJson(metricsDataProvider.CurrentMetricsData);
            jsonSize.Update(json.Length / 1024);
            WriteString(context, json, JsonBuilderV1.MetricsMimeType);
        }

        private static void WriteJsonMetricsV2(HttpListenerContext context, MetricsDataProvider metricsDataProvider)
        {
            var json = JsonBuilderV2.BuildJson(metricsDataProvider.CurrentMetricsData);
            jsonSize.Update(json.Length / 1024);
            WriteString(context, json, JsonBuilderV2.MetricsMimeType);
        }

        private static void WriteString(HttpListenerContext context, string data, string contentType)
        {
            AddCORSHeaders(context.Response);
            AddNoCacheHeaders(context.Response);

            context.Response.ContentType = contentType;
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";

            var acceptsGzip = AcceptsGzip(context.Request);
            if (!acceptsGzip)
            {
                using (var writer = new StreamWriter(context.Response.OutputStream))
                {
                    writer.Write(data);
                }
            }
            else
            {
                context.Response.AddHeader("Content-Encoding", "gzip");
                using (GZipStream gzip = new GZipStream(context.Response.OutputStream, CompressionMode.Compress))
                using (var writer = new StreamWriter(gzip))
                {
                    writer.Write(data);
                }
            }
        }


        private void WriteFavIcon(HttpListenerContext context)
        {
            context.Response.ContentType = FlotWebApp.FavIconMimeType;
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";

            FlotWebApp.WriteFavIcon(context.Response.OutputStream);
        }

        private static void WriteFlotApp(HttpListenerContext context)
        {
            context.Response.ContentType = "text/html";
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";

            bool acceptsGzip = AcceptsGzip(context.Request);

            if (acceptsGzip)
            {
                context.Response.AddHeader("Content-Encoding", "gzip");
            }

            FlotWebApp.WriteFlotAppAsync(context.Response.OutputStream, !acceptsGzip);
        }

        private static bool AcceptsGzip(HttpListenerRequest request)
        {
            string encoding = request.Headers["Accept-Encoding"];
            if (string.IsNullOrEmpty(encoding))
            {
                return false;
            }

            return encoding.Contains("gzip");
        }

        private static void AddNoCacheHeaders(HttpListenerResponse response)
        {
            response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            response.Headers.Add("Pragma", "no-cache");
            response.Headers.Add("Expires", "0");
        }

        private static void AddCORSHeaders(HttpListenerResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
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
