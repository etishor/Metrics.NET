using Metrics.MetricData;
using Metrics.Reporters;

namespace Metrics.Graphite
{
    public class GraphiteReport : BaseReport
    {
        private readonly GraphiteSender sender;

        public GraphiteReport(GraphiteSender sender)
        {
            this.sender = sender;
        }

        protected override void ReportGauge(string name, double value, Unit unit)
        {

        }

        protected override void ReportCounter(string name, CounterValue value, Unit unit)
        {

        }

        protected override void ReportMeter(string name, MeterValue value, Unit unit, TimeUnit rateUnit)
        {

        }

        protected override void ReportHistogram(string name, HistogramValue value, Unit unit)
        {

        }

        protected override void ReportTimer(string name, TimerValue value, Unit unit, TimeUnit rateUnit, TimeUnit durationUnit)
        {

        }

        protected override void ReportHealth(HealthStatus status)
        {
        }
    }
}
