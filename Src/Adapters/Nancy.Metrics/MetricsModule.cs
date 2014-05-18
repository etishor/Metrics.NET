using System;
using Metrics;
using Metrics.Core;
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
            public readonly MetricsRegistry Registry;
            public readonly Func<HealthStatus> HealthStatus;

            public ModuleConfig(MetricsRegistry registry, Func<HealthStatus> healthStatus, Action<INancyModule> moduleConfig, string metricsPath)
            {
                this.Registry = registry;
                this.HealthStatus = healthStatus;
                this.ModuleConfigAction = moduleConfig;
                this.ModulePath = metricsPath;
            }
        }
        private static ModuleConfig Config;

        public static void Configure(MetricsRegistry registry, Func<HealthStatus> healthStatus, Action<INancyModule> moduleConfig, string metricsPath)
        {
            MetricsModule.Config = new ModuleConfig(registry, healthStatus, moduleConfig, metricsPath);
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

            Get["/"] = _ => Response.AsText(FlotWebApp.GetFlotApp(this.Context.Request.Url), "text/html");
            Get["/text"] = _ => Response.AsText(GetAsHumanReadable());
            Get["/json"] = _ => Response.AsText(RegistrySerializer.GetAsJson(Config.Registry), "text/json");
            Get["/ping"] = _ => Response.AsText("pong", "text/plain");
            Get["/health"] = _ => GetHealthStatus();
        }

        private dynamic GetHealthStatus()
        {
            var status = Config.HealthStatus();
            var content = HealthCheckSerializer.Serialize(status);

            var response = Response.AsText(content, "application/json");
            response.StatusCode = status.IsHealty ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;
            return response;
        }

        private static string GetAsHumanReadable()
        {
            var report = new StringReporter();
            report.RunReport(Config.Registry, Config.HealthStatus);
            return report.Result;
        }
    }
}
