﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Metrics;
using Metrics.Json;
using Metrics.Reporters;
using Metrics.Utils;
using Metrics.Visualization;

namespace Owin.Metrics.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class MetricsEndpointMiddleware
    {
        private readonly OwinMetricsEndpointConfig endpointConfig;
        private readonly MetricsDataProvider dataProvider;
        private readonly Func<HealthStatus> healthStatus;

        private AppFunc next;

        public MetricsEndpointMiddleware(OwinMetricsEndpointConfig endpointConfig, MetricsDataProvider dataProvider, Func<HealthStatus> healthStatus)
        {
            this.endpointConfig = endpointConfig;
            this.dataProvider = dataProvider;
            this.healthStatus = healthStatus;
        }

        public void Initialize(AppFunc next)
        {
            this.next = next;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            var requestPath = environment["owin.RequestPath"] as string;

            if (requestPath.EndsWith("/" + endpointConfig.MetricsEndpointName) && endpointConfig.MetricsEndpointEnabled)
            {
                return GetFlotWebApp(environment);
            }

            if (requestPath.EndsWith("/" + endpointConfig.MetricsJsonEndpointName) && endpointConfig.MetricsJsonEndpointEnabled)
            {
                return GetJsonContent(environment, this.dataProvider);
            }

            if (requestPath.EndsWith("/" + endpointConfig.MetricsHealthEndpointName) && endpointConfig.MetricsHealthEndpointEnabled)
            {
                return GetHealthStatus(environment, this.healthStatus);
            }

            if (requestPath.EndsWith("/" + endpointConfig.MetricsTextEndpointName) && endpointConfig.MetricsTextEndpointEnabled)
            {
                return GetAsHumanReadable(environment, this.dataProvider, this.healthStatus);
            }

            if (requestPath.EndsWith("/" + endpointConfig.MetricsPingEndpointName) && endpointConfig.MetricsPingEndpointEnabled)
            {
                return GetPingContent(environment);
            }

            return next(environment);
        }

        private static Task GetFlotWebApp(IDictionary<string, object> environment)
        {
            var content = FlotWebApp.GetFlotApp();
            return WriteResponse(environment, content, "application/json");
        }

        private static Task GetJsonContent(IDictionary<string, object> environment, MetricsDataProvider dataProvider)
        {
            var content = JsonBuilderV1.BuildJson(dataProvider.CurrentMetricsData, Clock.Default);
            return WriteResponse(environment, content, "application/json");
        }

        private static Task GetHealthStatus(IDictionary<string, object> environment, Func<HealthStatus> healthStatus)
        {
			var responseStatusCode = HttpStatusCode.OK;
            var status = healthStatus();
            var content = JsonHealthChecks.BuildJson(status);
			if (!status.IsHealty) responseStatusCode = HttpStatusCode.InternalServerError;
            return WriteResponse(environment, content, "application/json", responseStatusCode);

        }

        private static Task GetAsHumanReadable(IDictionary<string, object> environment, MetricsDataProvider dataProvider, Func<HealthStatus> healthStatus)
        {
            string text = StringReporter.RenderMetrics(dataProvider.CurrentMetricsData, healthStatus);
            return WriteResponse(environment, text, "text/plain");
        }

        private static Task GetPingContent(IDictionary<string, object> environment)
        {
            return WriteResponse(environment, "pong", "text/plain");
        }

        private static async Task WriteResponse(IDictionary<string, object> environment, string content, string contentType, HttpStatusCode code = HttpStatusCode.OK)
        {
            var response = environment["owin.ResponseBody"] as Stream;
            var token = (CancellationToken)environment["owin.CallCancelled"];
            var headers = environment["owin.ResponseHeaders"] as IDictionary<string, string[]>;

            var contentBytes = Encoding.UTF8.GetBytes(content);

            headers["ContentType"] = new[] { contentType };

            environment["owin.ResponseStatusCode"] = (int)code;

            await response.WriteAsync(contentBytes, 0, contentBytes.Length);
        }
    }
}
