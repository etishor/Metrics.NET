using System;
using System.Text;
using System.Threading;
using Metrics.MetricData;

namespace Metrics.Reporters
{
    public class StringReporter : HumanReadableReporter
    {
        public static string RenderMetrics(MetricsData metricsData, Func<HealthStatus> healthStatus)
        {
            var report = new StringReporter();
            report.RunReport(metricsData, healthStatus, CancellationToken.None);
            return report.Result;
        }

        private readonly StringBuilder buffer = new StringBuilder();

        protected override void WriteLine(string line, params string[] args)
        {
            this.buffer.AppendLine(string.Format(line, args));
        }

        public string Result { get { return this.buffer.ToString(); } }
    }
}
