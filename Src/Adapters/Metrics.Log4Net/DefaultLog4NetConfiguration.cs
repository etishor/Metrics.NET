using System;
using System.IO;

namespace Metrics.Log4Net
{
    public static class DefaultLog4NetConfiguration
    {
        public static void ConfigureAndWatch(string logDirectory)
        {
            log4net.GlobalContext.Properties["Metrics.Log4Net.LogDirectory"] = logDirectory;

            log4net.Config.XmlConfigurator.ConfigureAndWatch(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\Metrics.Log4Net.config"));
        }
    }
}
