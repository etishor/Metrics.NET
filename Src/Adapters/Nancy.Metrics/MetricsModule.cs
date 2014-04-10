using System;
using Metrics;
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

            public ModuleConfig(MetricsRegistry registry, Action<INancyModule> moduleConfig, string metricsPath)
            {
                this.Registry = registry;
                this.ModuleConfigAction = moduleConfig;
                this.ModulePath = metricsPath;
            }
        }
        private static ModuleConfig Config;

        public static void Configure(MetricsRegistry registry, Action<INancyModule> moduleConfig, string metricsPath)
        {
            MetricsModule.Config = new ModuleConfig(registry, moduleConfig, metricsPath);
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

            Get["/"] = _ => Response.AsText(FlotWebApp.GetFlotApp(new Uri(this.Context.Request.Url, "json")), "text/html");
            Get["/text"] = _ => Response.AsText(Metric.GetAsHumanReadable());
            Get["/json"] = _ => Response.AsJson(new RegistrySerializer().GetForSerialization(Config.Registry));
        }
    }
}
