using Metrics.Influxdb;
using Metrics.Reports;
using System;

namespace Metrics
{
    public static class InfluxdbConfigExtensions
    {
        public static MetricsReports WithInfluxDb(this MetricsReports reports, string host, int port, string user, string pass, string database, TimeSpan interval)
        {
            return reports.WithInfluxDb(new Uri(string.Format(@"http://{0}:{1}/db/{2}/series?u={3}&p={4}&time_precision=s", host, port, database, user, pass)), interval);
        }

        public static MetricsReports WithInfluxDb(this MetricsReports reports, Uri influxdbUri, TimeSpan interval)
        {
            return reports.WithReport(new InfluxdbReport(influxdbUri), interval);
        }
    }
}
