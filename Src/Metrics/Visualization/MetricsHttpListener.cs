
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
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

        private Task processingTask;

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

        private static string ParsePrefixPath(string listenerUriPrefix)
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
            this.processingTask = Task.Factory.StartNew(async () => await ProcessRequests(), TaskCreationOptions.LongRunning);
        }

        private async Task ProcessRequests()
        {
            while (!this.cts.IsCancellationRequested)
            {
                try
                {
                    var context = await this.httpListener.GetContextAsync();
                    try
                    {
                        using (timer.NewContext())
                        {
                            await ProcessRequest(context).ConfigureAwait(false);
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
                catch (ObjectDisposedException ex)
                {
                    if ((ex.ObjectName == this.httpListener.GetType().FullName) && (this.httpListener.IsListening == false))
                    {
                        return; // listener is closed/disposed
                    }
                    MetricsErrorHandler.Handle(ex, "Error processing HTTP request");
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

        private Task ProcessRequest(HttpListenerContext context)
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
                        context.Response.Redirect(context.Request.Url + "/");
                        context.Response.Close();
                        return Task.FromResult(0);
                    }
                    else
                    {
                        return WriteFlotApp(context);
                    }
                case "/favicon.ico":
                    return WriteFavIcon(context);
                case "/json":
                    return WriteJsonMetrics(context, this.metricsDataProvider);
                case "/v1/json":
                    return WriteJsonMetricsV1(context, this.metricsDataProvider);
                case "/v2/json":
                    return WriteJsonMetricsV2(context, this.metricsDataProvider);

                case "/health":
                    return WriteHealthStatus(context, this.healthStatus);
                case "/v1/health":
                    return WriteHealthStatus(context, this.healthStatus);

                case "/text":
                    return WriteTextMetrics(context, this.metricsDataProvider, this.healthStatus);
                case "/ping":
                    return WritePong(context);
            }
            return WriteNotFound(context);
        }

        private static Task WriteHealthStatus(HttpListenerContext context, Func<HealthStatus> healthStatus)
        {
            var status = healthStatus();
            var json = JsonHealthChecks.BuildJson(status);

            var httpStatus = status.IsHealthy ? 200 : 500;
            var httpStatusDescription = status.IsHealthy ? "OK" : "Internal Server Error";

            return WriteString(context, json, JsonHealthChecks.HealthChecksMimeType, httpStatus, httpStatusDescription);
        }

        private static Task WritePong(HttpListenerContext context)
        {
            return WriteString(context, "pong", "text/plain");
        }

        private static Task WriteNotFound(HttpListenerContext context)
        {
            return WriteString(context, NotFoundResponse, "text/plain", 404, "NOT FOUND");
        }

        private static Task WriteTextMetrics(HttpListenerContext context, MetricsDataProvider metricsDataProvider, Func<HealthStatus> healthStatus)
        {
            var text = StringReport.RenderMetrics(metricsDataProvider.CurrentMetricsData, healthStatus);
            return WriteString(context, text, "text/plain");
        }

        private static Task WriteJsonMetrics(HttpListenerContext context, MetricsDataProvider metricsDataProvider)
        {
            var acceptHeader = context.Request.Headers["Accept"] ?? string.Empty;

            if (acceptHeader.Contains(JsonBuilderV2.MetricsMimeType))
            {
                return WriteJsonMetricsV2(context, metricsDataProvider);
            }

            return WriteJsonMetricsV1(context, metricsDataProvider);
        }

        private static Task WriteJsonMetricsV1(HttpListenerContext context, MetricsDataProvider metricsDataProvider)
        {
            var json = JsonBuilderV1.BuildJson(metricsDataProvider.CurrentMetricsData);
            jsonSize.Update(json.Length / 1024);
            return WriteString(context, json, JsonBuilderV1.MetricsMimeType);
        }

        private static Task WriteJsonMetricsV2(HttpListenerContext context, MetricsDataProvider metricsDataProvider)
        {
            var json = JsonBuilderV2.BuildJson(metricsDataProvider.CurrentMetricsData);
            jsonSize.Update(json.Length / 1024);
            return WriteString(context, json, JsonBuilderV2.MetricsMimeType);
        }

        private static async Task WriteString(HttpListenerContext context, string data, string contentType,
            int httpStatus = 200, string httpStatusDescription = "OK")
        {
            AddCORSHeaders(context.Response);
            AddNoCacheHeaders(context.Response);

            context.Response.ContentType = contentType;
            context.Response.StatusCode = httpStatus;
            context.Response.StatusDescription = httpStatusDescription;

            var acceptsGzip = AcceptsGzip(context.Request);
            if (!acceptsGzip)
            {
                using (var writer = new StreamWriter(context.Response.OutputStream, Encoding.UTF8, 4096, true))
                {
                    await writer.WriteAsync(data).ConfigureAwait(false);
                }
            }
            else
            {
                context.Response.AddHeader("Content-Encoding", "gzip");
                using (GZipStream gzip = new GZipStream(context.Response.OutputStream, CompressionMode.Compress, true))
                using (var writer = new StreamWriter(gzip, Encoding.UTF8, 4096, true))
                {
                    await writer.WriteAsync(data).ConfigureAwait(false);
                }
            }
        }

        private static Task WriteFavIcon(HttpListenerContext context)
        {
            context.Response.ContentType = FlotWebApp.FavIconMimeType;
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";

            return FlotWebApp.WriteFavIcon(context.Response.OutputStream);
        }

        private static Task WriteFlotApp(HttpListenerContext context)
        {
            context.Response.ContentType = "text/html";
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";

            bool acceptsGzip = AcceptsGzip(context.Request);

            if (acceptsGzip)
            {
                context.Response.AddHeader("Content-Encoding", "gzip");
            }

            return FlotWebApp.WriteFlotAppAsync(context.Response.OutputStream, !acceptsGzip);
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

        private void Stop()
        {
            cts.Cancel();
            if (processingTask != null && !processingTask.IsCompleted)
            {
                processingTask.Wait();
            }
            if (this.httpListener.IsListening)
            {
                this.httpListener.Stop();
                this.httpListener.Prefixes.Clear();
            }
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
