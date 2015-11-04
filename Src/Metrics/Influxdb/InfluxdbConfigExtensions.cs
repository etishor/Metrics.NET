using Metrics.Influxdb;
using Metrics.Reports;
using System;

namespace Metrics
{
    public static class InfluxdbConfigExtensions
    {
        public static MetricsReports WithInfluxDb(this MetricsReports reports, string host, int port, string user, string pass, string database, TimeSpan interval)
        {
            return reports.WithInfluxDb(new Uri(string.Format(@"http://{0}:{1}/write?db={2}", host, port, database)), user, pass, interval);
        }

        public static MetricsReports WithInfluxDb(this MetricsReports reports, Uri influxdbUri, string username, string password, TimeSpan interval)
        {
            return reports.WithReport(new InfluxdbReport(influxdbUri, username, password), interval);
        }
    }
}
