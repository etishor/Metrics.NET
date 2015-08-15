using System;
using Metrics.ElasticSearch;
using Metrics.Reports;

namespace Metrics
{
    public static class ElasticSearchConfigExtensions
    {
        public static MetricsReports WithElasticSearch(this MetricsReports reports, string host, int port, string index, TimeSpan interval)
        {
            return reports.WithElasticSearch(host, port, index, null, null, interval);
        }

        public static MetricsReports WithElasticSearch(this MetricsReports reports, string host, int port, string index, string username, string password, TimeSpan interval)
        {
            var uri = new Uri(string.Format(@"http://{0}:{1}/_bulk", host, port));
            return reports.WithReport(new ElasticSearchReport(uri, username, password, index), interval);
        }
    }
}
