using System;
using Metrics.ElasticSearch;
using Metrics.Reports;

namespace Metrics
{
    public static class ElasticSearchConfigExtensions
    {
        public static MetricsReports WithElasticSearch(this MetricsReports reports, string host, int port, string index, TimeSpan interval)
        {
            var uri = new Uri($@"http://{host}:{port}/_bulk");
            return reports.WithReport(new ElasticSearchReport(uri, index), interval);
        }
    }
}
