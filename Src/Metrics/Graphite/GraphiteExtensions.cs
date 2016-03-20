using System;
using Metrics.Graphite;
using Metrics.Reports;

namespace Metrics
{
    public static class GraphiteExtensions
    {
        public static MetricsReports WithGraphite(this MetricsReports reports, Uri graphiteUri, TimeSpan interval,
            string metricNamePrefix = null)
        {
            if (graphiteUri.Scheme.ToLowerInvariant() == "net.tcp")
            {
                return reports.WithTCPGraphite(graphiteUri.Host, graphiteUri.Port, interval, metricNamePrefix);
            }

            if (graphiteUri.Scheme.ToLowerInvariant() == "net.udp")
            {
                return reports.WithUDPGraphite(graphiteUri.Host, graphiteUri.Port, interval, metricNamePrefix);
            }

            if (graphiteUri.Scheme.ToLowerInvariant() == "net.pickled")
            {
                return reports.WithPickledGraphite(graphiteUri.Host, graphiteUri.Port, interval, PickleGraphiteSender.DefaultPickleJarSize, metricNamePrefix);
            }

            throw new ArgumentException("Graphite uri scheme must be either net.tcp or net.udp or net.pickled (ex: net.udp://graphite.myhost.com:2003 )", "graphiteUri");
        }

        public static MetricsReports WithPickledGraphite(this MetricsReports reports, string host, int port, TimeSpan interval, int batchSize = PickleGraphiteSender.DefaultPickleJarSize,
            string metricNamePrefix = null)
        {
            return reports.WithGraphite(new PickleGraphiteSender(host, port, batchSize, metricNamePrefix), interval);
        }

        public static MetricsReports WithTCPGraphite(this MetricsReports reports, string host, int port, TimeSpan interval, string metricNamePrefix = null)
        {
            return reports.WithGraphite(new TcpGraphiteSender(host, port, metricNamePrefix), interval);
        }

        public static MetricsReports WithUDPGraphite(this MetricsReports reports, string host, int port, TimeSpan interval, string metricNamePrefix = null)
        {
            return reports.WithGraphite(new UdpGraphiteSender(host, port, metricNamePrefix), interval);
        }

        public static MetricsReports WithGraphite(this MetricsReports reports, GraphiteSender graphiteLink, TimeSpan interval)
        {
            return reports.WithReport(new GraphiteReport(graphiteLink), interval);
        }
    }
}
