using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Metrics.MetricData;
using Metrics.Logging;

namespace Metrics.Utils
{
    public static class AppEnvironment
    {
		private static readonly ILog log = LogProvider.GetCurrentClassLogger();

        public static IEnumerable<EnvironmentEntry> Current
        {
            get
            {
                yield return new EnvironmentEntry("MachineName", Environment.MachineName);
                yield return new EnvironmentEntry("DomainName", Environment.UserDomainName);
                yield return new EnvironmentEntry("UserName", Environment.UserName);
                yield return new EnvironmentEntry("ProcessName", SafeGetString(() => Process.GetCurrentProcess().ProcessName));
                yield return new EnvironmentEntry("OSVersion", Environment.OSVersion.ToString());
                yield return new EnvironmentEntry("CPUCount", Environment.ProcessorCount.ToString());
                yield return new EnvironmentEntry("CommandLine", Environment.CommandLine);
                yield return new EnvironmentEntry("HostName", SafeGetString(Dns.GetHostName));
                yield return new EnvironmentEntry("IPAddress", SafeGetString(GetIpAddress));
                yield return new EnvironmentEntry("LocalTime", Clock.FormatTimestamp(DateTime.Now));
            }
        }

        private static string GetIpAddress()
        {
			string hostName = SafeGetString(Dns.GetHostName);
			try
			{
				var ipAddress = Dns.GetHostEntry(hostName)
	                .AddressList
	                .FirstOrDefault (ip => ip.AddressFamily == AddressFamily.InterNetwork);

				if (ipAddress != null) 
				{
					return ipAddress.ToString();
				}
				return string.Empty;
			}
			catch (SocketException x) 
			{
				if (x.SocketErrorCode == SocketError.HostNotFound) 
				{
					log.Warn ( () => "Unable to resolve hostname " + hostName);
					return string.Empty;
				}
				throw;
			}
        }

        private static string SafeGetString(Func<string> action)
        {
            try
            {
                return action();
            }
            catch (Exception x)
            {
                MetricsErrorHandler.Handle(x, "Error retrieving environment value");
                return string.Empty;
            }
        }
    }
}
