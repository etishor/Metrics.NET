using System;
using Metrics.Graphite;
using Metrics.Reports;

namespace Metrics
{
    public static class GraphiteExtensions
    {
        public static MetricsReports WithGraphite(this MetricsReports reports, string host, int port, TimeSpan interval)
        {
            reports.WithReporter(() => new GraphiteReport(new UdpGraphiteSender(host, port)), interval);
            return reports;
        }
    }
}
