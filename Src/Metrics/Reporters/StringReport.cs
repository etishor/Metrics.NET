using System;
using System.Text;
using System.Threading;
using Metrics.MetricData;

namespace Metrics.Reporters
{
    public class StringReport : HumanReadableReport
    {
        public static string RenderMetrics(MetricsData metricsData, Func<HealthStatus> healthStatus)
        {
            var report = new StringReport();
            report.RunReport(metricsData, healthStatus, CancellationToken.None);
            return report.Result;
        }

        private StringBuilder buffer = null;

        protected override void StartReport(string contextName, DateTime timestamp)
        {
            this.buffer = new StringBuilder();
            base.StartReport(contextName, timestamp);
        }
        protected override void WriteLine(string line, params string[] args)
        {
            this.buffer.AppendLine(string.Format(line, args));
        }

        protected override void EndReport(string contextName, DateTime timestamp)
        {
            base.EndReport(contextName, timestamp);
        }

        public string Result { get { return this.buffer.ToString(); } }
    }
}
