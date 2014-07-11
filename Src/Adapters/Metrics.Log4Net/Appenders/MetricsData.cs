using System.Collections.Generic;
using Metrics.Reporters;

namespace Metrics.Log4Net.Appenders
{
    public class MetricsData
    {
        public string MetricType { get; set; }

        public string MetricName { get; set; }

        public IEnumerable<CSVReporter.Value> Values { get; set; }

        public MetricsData()
        {
            Values = new List<CSVReporter.Value>();
        }
    }
}