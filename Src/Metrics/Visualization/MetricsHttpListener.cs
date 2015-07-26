
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Metrics.Json;
using Metrics.Logging;
using Metrics.MetricData;
using Metrics.Reporters;


namespace Metrics.Visualization
{
    public sealed class MetricsHttpListener : IDisposable
    {
        private static readonly ILog log = LogProvider.GetCurrentClassLogger();

        private const string NotFoundResponse = "<!doctype html><html><body>Resource not found</body></html>";
        private readonly HttpListener httpListener;
        private readonly CancellationTokenSource cts;
        private readonly MetricsDataProvider metricsDataProvider;
        private readonly Func<HealthStatus> healthStatus;
        private readonly string prefixPath;

        private Task processingTask;

        private static readonly Timer jsonv1Metrics = Metric.Internal.Context("HTTP").Timer("Json V1 Metrics", Unit.Requests);
        private static readonly Histogram jsonv1Size = Metric.Internal.Context("HTTP").Histogram("JSON V1 Size", Unit.KiloBytes);
        private static readonly Timer jsonv2Metrics = Metric.Internal.Context("HTTP").Timer("Json v2 Metrics", Unit.Requests);
        private static readonly Histogram jsonv2Size = Metric.Internal.Context("HTTP").Histogram("JSON V2 Size", Unit.KiloBytes);

        private static readonly Timer timer = Metric.Internal.Context("HTTP").Timer("Request", Unit.Requests);
        private static readonly Meter errors = Metric.Internal.Context("HTTP").Meter("Request Errors", Unit.Errors);

        public MetricsHttpListener(string listenerUriPrefix, MetricsDataProvider metricsDataProvider, Func<HealthStatus> healthStatus, CancellationToken token)
        {
            this.cts = CancellationTokenSource.CreateLinkedTokenSource(token);

            this.prefixPath = ParsePrefixPath(listenerUriPrefix);
            this.httpListener = new HttpListener();
            this.httpListener.Prefixes.Add(listenerUriPrefix);
            this.metricsDataProvider = metricsDataProvider;
            this.healthStatus = healthStatus;
        }

        public static Task<MetricsHttpListener> StartHttpListenerAsync(string httpUriPrefix, MetricsDataProvider dataProvider,
            Func<HealthStatus> healthStatus, CancellationToken token, int maxRetries = 1)
        {
            return Task.Factory.StartNew(async () =>
            {
                MetricsHttpListener listener = null;
                var remainingRetries = maxRetries;
                do
                {
                    try
                    {
                        listener = new MetricsHttpListener(httpUriPrefix, dataProvider, healthStatus, token);
                        listener.Start();
                        if (remainingRetries != maxRetries)
                        {
                            log.InfoFormat("HttpListener started successfully after {0} retries", maxRetries - remainingRetries);
                        }
                        remainingRetries = 0;
                    }
                    catch (Exception x)
                    {
                        using (listener) { }
                        listener = null;
                        remainingRetries--;
                        if (remainingRetries > 0)
                        {
                            log.WarnException("Unable to start HTTP Listener. Sleeping for {0} sec and retrying {1} more times", x, maxRetries - remainingRetries, remainingRetries);
                            await Task.Delay(1000*(maxRetries - remainingRetries), token).ConfigureAwait(false);
                        }
                        else
                        {
                            MetricsErrorHandler.Handle(x, $"Unable to start HTTP Listener. Retried {maxRetries} times, giving up...");
                        }
                    }
                } while (remainingRetries > 0);
                return listener;
            }, token).Unwrap();
        }

        private static string ParsePrefixPath(string listenerUriPrefix)
        {
            var match = Regex.Match(listenerUriPrefix, @"http://(?:[^/]*)(?:\:\d+)?/(.*)");
            return match.Success ? match.Groups[1].Value.ToLowerInvariant() : string.Empty;
        }

        public void Start()
        {
            this.httpListener.Start();
            this.processingTask = Task.Factory.StartNew(ProcessRequests,this.cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void ProcessRequests()
        {
            while (!this.cts.IsCancellationRequested)
            {
                try
                {
                    var context = this.httpListener.GetContext();
                    try
                    {
                        using (timer.NewContext())
                        {
                            ProcessRequest(context);
                            using (context.Response.OutputStream) { }
                            using (context.Response) { }
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
                    var httpException = ex as HttpListenerException;
                    if (httpException == null || httpException.ErrorCode != 995)// IO operation aborted
                    {
                        MetricsErrorHandler.Handle(ex, "Error processing HTTP request");
                    }
                }
            }
        }

        private bool ProcessRequest(HttpListenerContext context)
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
                        return true;
                    }
                    else
                    {
                        return WriteFlotApp(context, this.cts.Token);
                    }
                case "/favicon.ico":
                    return WriteFavIcon(context, this.cts.Token);
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

        private static bool WriteHealthStatus(HttpListenerContext context, Func<HealthStatus> healthStatus)
        {
            var status = healthStatus();
            var json = JsonHealthChecks.BuildJson(status);

            var httpStatus = status.IsHealthy ? 200 : 500;
            var httpStatusDescription = status.IsHealthy ? "OK" : "Internal Server Error";

            return WriteString(context, json, JsonHealthChecks.HealthChecksMimeType, httpStatus, httpStatusDescription);
        }

        private static bool WritePong(HttpListenerContext context)
        {
            return WriteString(context, "pong", "text/plain");
        }

        private static bool WriteNotFound(HttpListenerContext context)
        {
            return WriteString(context, NotFoundResponse, "text/plain", 404, "NOT FOUND");
        }

        private static bool WriteTextMetrics(HttpListenerContext context, MetricsDataProvider metricsDataProvider, Func<HealthStatus> healthStatus)
        {
            var text = StringReport.RenderMetrics(metricsDataProvider.CurrentMetricsData, healthStatus);
            return WriteString(context, text, "text/plain");
        }

        private static bool WriteJsonMetrics(HttpListenerContext context, MetricsDataProvider metricsDataProvider)
        {
            var acceptHeader = context.Request.Headers["Accept"] ?? string.Empty;

            if (acceptHeader.Contains(JsonBuilderV2.MetricsMimeType))
            {
                return WriteJsonMetricsV2(context, metricsDataProvider);
            }

            return WriteJsonMetricsV1(context, metricsDataProvider);
        }

        private static bool WriteJsonMetricsV1(HttpListenerContext context, MetricsDataProvider metricsDataProvider)
        {
            using (jsonv1Metrics.NewContext())
            {
                var json = JsonBuilderV1.BuildJson(metricsDataProvider.CurrentMetricsData);
                jsonv1Size.Update(json.Length/1024);
                return WriteString(context, json, JsonBuilderV1.MetricsMimeType);
            }
        }

        private static bool WriteJsonMetricsV2(HttpListenerContext context, MetricsDataProvider metricsDataProvider)
        {
            using (jsonv2Metrics.NewContext())
            {
                var json = JsonBuilderV2.BuildJson(metricsDataProvider.CurrentMetricsData);
                jsonv2Size.Update(json.Length/1024);
                return WriteString(context, json, JsonBuilderV2.MetricsMimeType);
            }
        }

        private static bool WriteString(HttpListenerContext context, string data, string contentType,
            int httpStatus = 200, string httpStatusDescription = "OK")
        {
            AddCorsHeaders(context.Response);
            AddNoCacheHeaders(context.Response);

            context.Response.ContentType = contentType;
            context.Response.StatusCode = httpStatus;
            context.Response.StatusDescription = httpStatusDescription;

            var acceptsGzip = AcceptsGzip(context.Request);
            if (!acceptsGzip)
            {
                using (var writer = new StreamWriter(context.Response.OutputStream, Encoding.UTF8, 4096, true))
                {
                    writer.Write(data);
                }
            }
            else
            {
                context.Response.AddHeader("Content-Encoding", "gzip");
                using (var gzip = new GZipStream(context.Response.OutputStream, CompressionMode.Compress, true))
                using (var writer = new StreamWriter(gzip, Encoding.UTF8, 4096, true))
                {
                    writer.Write(data);
                }
            }
            return true;
        }

        private static bool WriteFavIcon(HttpListenerContext context,CancellationToken token)
        {
            context.Response.ContentType = FlotWebApp.FavIconMimeType;
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";

            FlotWebApp.WriteFavIcon(context.Response.OutputStream);
            return true;
        }

        private static bool WriteFlotApp(HttpListenerContext context, CancellationToken token)
        {
            context.Response.ContentType = "text/html";
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";

            var acceptsGzip = AcceptsGzip(context.Request);

            if (acceptsGzip)
            {
                context.Response.AddHeader("Content-Encoding", "gzip");
            }

            FlotWebApp.WriteFlotAppAsync(context.Response.OutputStream, !acceptsGzip);
            return true;
        }

        private static bool AcceptsGzip(HttpListenerRequest request)
        {
            var encoding = request.Headers["Accept-Encoding"];
            return !string.IsNullOrEmpty(encoding) && encoding.Contains("gzip");
        }

        private static void AddNoCacheHeaders(HttpListenerResponse response)
        {
            response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            response.Headers.Add("Pragma", "no-cache");
            response.Headers.Add("Expires", "0");
        }

        private static void AddCorsHeaders(HttpListenerResponse response)
        {
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");
        }

        private void Stop()
        {
            if (!this.cts.IsCancellationRequested)
            {
                this.cts.Cancel();
            }
            if (this.processingTask != null && !this.processingTask.IsCompleted)
            {
                this.processingTask.Wait(1000);
            }
            if (this.httpListener.IsListening)
            {
                this.httpListener.Stop();
                this.httpListener.Prefixes.Clear();
            }
        }

        public void Dispose()
        {
            Stop();
            this.httpListener.Close();
            using (this.cts) { }
            using (this.httpListener) { }
        }
    }
}
