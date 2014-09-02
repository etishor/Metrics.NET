using System;
using Metrics;
using Metrics.Json;
using Metrics.Reporters;
using Metrics.Visualization;

namespace Nancy.Metrics
{
    public class MetricsModule : NancyModule
    {
        private struct ModuleConfig
        {
            public readonly string ModulePath;
            public readonly Action<INancyModule> ModuleConfigAction;
            public readonly MetricsContext MetricsContext;
            public readonly Func<HealthStatus> HealthStatus;

            public ModuleConfig(MetricsContext metricsContext, Func<HealthStatus> healthStatus, Action<INancyModule> moduleConfig, string metricsPath)
            {
                this.MetricsContext = metricsContext;
                this.HealthStatus = healthStatus;
                this.ModuleConfigAction = moduleConfig;
                this.ModulePath = metricsPath;
            }
        }

        private static ModuleConfig Config;
        private static bool healthChecksAlwaysReturnHttpStatusOk = false;

        public static void Configure(MetricsContext metricsContext, Func<HealthStatus> healthStatus, Action<INancyModule> moduleConfig, string metricsPath)
        {
            MetricsModule.Config = new ModuleConfig(metricsContext, healthStatus, moduleConfig, metricsPath);
        }

        public static void ConfigureHealthChecks(bool alwaysReturnOk)
        {
            healthChecksAlwaysReturnHttpStatusOk = alwaysReturnOk;
        }

        public MetricsModule()
            : base(Config.ModulePath ?? "/")
        {
            if (string.IsNullOrEmpty(Config.ModulePath))
            {
                return;
            }

            if (Config.ModuleConfigAction != null)
            {
                Config.ModuleConfigAction(this);
            }

            var noCacheHeaders = new[] { 
                new { Header = "Cache-Control", Value = "no-cache, no-store, must-revalidate" },
                new { Header = "Pragma", Value = "no-cache" },
                new { Header = "Expires", Value = "0" }
            };

            Get["/"] = _ =>
            {
                if (this.Request.Url.Path.EndsWith("/"))
                {
                    return Response.AsText(FlotWebApp.GetFlotApp(), "text/html");
                }
                else
                {
                    return Response.AsRedirect(this.Request.Url.ToString() + "/");
                }
            };

            Get["/text"] = _ => Response.AsText(GetAsHumanReadable()).WithHeaders(noCacheHeaders);
            Get["/jsonold"] = _ => Response.AsText(RegistrySerializer.GetAsJson(Config.MetricsContext.MetricsData), "text/json").WithHeaders(noCacheHeaders);
            Get["/json"] = _ => Response.AsText(JsonMetrics.Serialize(Config.MetricsContext.MetricsData), "text/json").WithHeaders(noCacheHeaders);
            Get["/ping"] = _ => Response.AsText("pong", "text/plain").WithHeaders(noCacheHeaders);
            Get["/health"] = _ => GetHealthStatus().WithHeaders(noCacheHeaders);
        }

        private Response GetHealthStatus()
        {
            var status = Config.HealthStatus();
            var content = HealthCheckSerializer.Serialize(status);

            var response = Response.AsText(content, "application/json");
            if (!healthChecksAlwaysReturnHttpStatusOk)
            {
                response.StatusCode = status.IsHealty ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;
            }
            else
            {
                response.StatusCode = HttpStatusCode.OK;
            }
            return response;
        }

        private static string GetAsHumanReadable()
        {
            var report = new StringReporter();
            report.RunReport(Config.MetricsContext.MetricsData, Config.HealthStatus);
            return report.Result;
        }

    }
}
