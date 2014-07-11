namespace Metrics.Log4Net.Layout
{
    public class CsvGaugeLayout : CsvLayout
    {
        public CsvGaugeLayout()
        {
            this.AddGaugeColumns();

        }
    }

    public class CsvCounterLayout : CsvLayout
    {
        public CsvCounterLayout()
        {
            this.AddCounterColumns();

        }
    }

    public class CsvMeterLayout : CsvLayout
    {
        public CsvMeterLayout()
        {
            this.AddMeterColumns();

        }
    }

    public class CsvHistogramLayout : CsvLayout
    {
        public CsvHistogramLayout()
        {
            this.AddHistogramColumns();

        }
    }

    public class CsvTimerLayout : CsvLayout
    {
        public CsvTimerLayout()
        {
            this.AddTimerColumns();
        }
    }
}
