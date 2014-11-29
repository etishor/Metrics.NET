using System;
using Metrics.Influxdb;
using Metrics.Reports;

namespace Metrics
{
    public static class InfluxdbConfigExtensions
    {
        public static MetricsReports WithInfluxDb(this MetricsReports reports, Uri influxdbUri, TimeSpan interval)
        {
            return reports.WithReport(new InfluxdbReport(influxdbUri), interval);
        }
    }
}
