
namespace Metrics.NLog
{
    using CsvLayout = global::NLog.Layouts.CsvLayout;
    using NLog = global::NLog;

    [NLog.Config.AppDomainFixedOutput]
    [NLog.Layouts.Layout("CsvGaugeLayout")]
    [NLog.Config.ThreadAgnostic]
    public class CsvGaugeLayout : CsvLayout
    {
        public CsvGaugeLayout()
        {
            this.AddGaugeColumns();

        }
    }

    [NLog.Config.AppDomainFixedOutput]
    [NLog.Layouts.Layout("CsvCounterLayout")]
    [NLog.Config.ThreadAgnostic]
    public class CsvCounterLayout : CsvLayout
    {
        public CsvCounterLayout()
        {
            this.AddCounterColumns();

        }
    }

    [NLog.Config.AppDomainFixedOutput]
    [NLog.Layouts.Layout("CsvMeterLayout")]
    [NLog.Config.ThreadAgnostic]
    public class CsvMeterLayout : CsvLayout
    {
        public CsvMeterLayout()
        {
            this.AddMeterColumns();

        }
    }

    [NLog.Config.AppDomainFixedOutput]
    [NLog.Layouts.Layout("CsvHistogramLayout")]
    [NLog.Config.ThreadAgnostic]
    public class CsvHistogramLayout : CsvLayout
    {
        public CsvHistogramLayout()
        {
            this.AddHistogramColumns();

        }
    }

    [NLog.Config.AppDomainFixedOutput]
    [NLog.Layouts.Layout("CsvTimerLayout")]
    [NLog.Config.ThreadAgnostic]
    public class CsvTimerLayout : CsvLayout
    {
        public CsvTimerLayout()
        {
            this.AddTimerColumns();

        }
    }
}
