using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Metrics.Utils
{
    public class AppEnvironment
    {
        public struct Entry
        {
            public readonly string Name;
            public readonly string Value;

            public Entry(string name, string value)
            {
                this.Name = name;
                this.Value = value;
            }
        }

        public static IEnumerable<Entry> Current
        {
            get
            {
                yield return new Entry("MachineName", Environment.MachineName);
                yield return new Entry("DomainName", Environment.UserDomainName);
                yield return new Entry("OSVersion", Environment.OSVersion.ToString());
                yield return new Entry("HostName", SafeGetString(() => Dns.GetHostName()));
                yield return new Entry("IPAddress", SafeGetString(() => GetIpAddress()));
            }
        }

        private static string GetIpAddress()
        {
            var ipAddress = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            if (ipAddress != null)
            {
                return ipAddress.ToString();
            }
            return string.Empty;
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
