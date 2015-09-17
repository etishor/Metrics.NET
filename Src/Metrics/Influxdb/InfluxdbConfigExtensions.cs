using Metrics.Influxdb;
using Metrics.Reports;
using System;

namespace Metrics
{
    public static class InfluxdbConfigExtensions
    {
        public static MetricsReports WithInfluxDb(this MetricsReports reports, string host, int port, string user, string pass, string database, TimeSpan interval)
        {
            return reports.WithInfluxDb(new Uri($@"http://{host}:{port}/db/{database}/series?u={user}&p={pass}&time_precision=s"), interval);
        }

        public static MetricsReports WithInfluxDb(this MetricsReports reports, Uri influxdbUri, TimeSpan interval)
        {
            return reports.WithReport(new InfluxdbReport(influxdbUri), interval);
        }
    }
}
