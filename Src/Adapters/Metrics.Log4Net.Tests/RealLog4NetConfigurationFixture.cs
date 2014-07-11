using System;
using System.IO;
using log4net;

namespace Metrics.Log4Net.Tests
{
    public class RealLog4NetConfigurationFixture : IDisposable
    {
        private readonly DirectoryInfo directoryInfo;

        public RealLog4NetConfigurationFixture()
        {
            directoryInfo = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"\LogTests");

            DefaultLog4NetConfiguration.ConfigureAndWatch(directoryInfo.FullName);
        }

        public string LogsDirectory
        {
            get { return directoryInfo.FullName; }
        }

        public void Dispose()
        {
            LogManager.Shutdown();

            directoryInfo.Delete(true);
        }
    }
}