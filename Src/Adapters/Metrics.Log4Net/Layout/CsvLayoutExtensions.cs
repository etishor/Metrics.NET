using System;
using System.Linq;

namespace Metrics.Log4Net.Layout
{
    public static class CsvLayoutExtensions
    {
        public static void AddMetricNameAndTypeColumns(this CsvLayout layout)
        {
            if (layout == null) throw new ArgumentNullException("layout");
            if (layout.Columns.All(l => l.ColumnName != "Metric Name"))
            {
                layout.Columns.Add(new CsvColumn("Metric Name", "MetricName"));
            }
            if (layout.Columns.All(l => l.ColumnName != "Metric Type"))
            {
                layout.Columns.Add(new CsvColumn("Metric Type", "MetricType"));
            }
        }

        public static void AddDateColumns(this CsvLayout layout)
        {
            if (layout == null) throw new ArgumentNullException("layout");
            if (layout.Columns.All(l => l.ColumnName != "Date"))
            {
                layout.Columns.Add(new CsvColumn("Date", "Date"));
            }
            if (layout.Columns.All(l => l.ColumnName != "Ticks"))
            {
                layout.Columns.Add(new CsvColumn("Ticks", "Ticks"));
            }
        }

        public static void AddGaugeColumns(this CsvLayout layout)
        {
            if (layout == null) throw new ArgumentNullException("layout");
            AddMetricNameAndTypeColumns(layout);
            AddDateColumns(layout);
            layout.Columns.Add(new CsvColumn("Value", "Value"));
            layout.Columns.Add(new CsvColumn("Unit", "Unit"));
        }

        public static void AddCounterColumns(this CsvLayout layout)
        {
            if (layout == null) throw new ArgumentNullException("layout");
            AddMetricNameAndTypeColumns(layout);
            AddDateColumns(layout);
            layout.Columns.Add(new CsvColumn("Count", "Count"));
            layout.Columns.Add(new CsvColumn("Unit", "Unit"));
        }

        public static void AddMeterColumns(this CsvLayout layout)
        {
            if (layout == null) throw new ArgumentNullException("layout");
            AddMetricNameAndTypeColumns(layout);
            AddDateColumns(layout);
            layout.Columns.Add(new CsvColumn("Count", "Count"));
            layout.Columns.Add(new CsvColumn("Mean Rate", "Mean Rate"));
            layout.Columns.Add(new CsvColumn("One Minute Rate", "One Minute Rate"));
            layout.Columns.Add(new CsvColumn("Five Minute Rate", "Five Minute Rate"));
            layout.Columns.Add(new CsvColumn("Fifteen Minute Rate", "Fifteen Minute Rate"));
            layout.Columns.Add(new CsvColumn("Rate Unit", "Rate Unit"));
        }

        public static void AddHistogramColumns(this CsvLayout layout)
        {
            if (layout == null) throw new ArgumentNullException("layout");
            AddMetricNameAndTypeColumns(layout);
            AddDateColumns(layout);
            layout.Columns.Add(new CsvColumn("Last", "Last"));
            layout.Columns.Add(new CsvColumn("Min", "Min"));
            layout.Columns.Add(new CsvColumn("Max", "Unit"));
            layout.Columns.Add(new CsvColumn("Mean", "Mean"));
            layout.Columns.Add(new CsvColumn("StdDev", "StdDev"));
            layout.Columns.Add(new CsvColumn("Median", "Median"));
            layout.Columns.Add(new CsvColumn("75%", "75%"));
            layout.Columns.Add(new CsvColumn("95%", "95%"));
            layout.Columns.Add(new CsvColumn("98%", "98%"));
            layout.Columns.Add(new CsvColumn("99%", "99%"));
            layout.Columns.Add(new CsvColumn("99.9%", "99.9%"));
            layout.Columns.Add(new CsvColumn("Unit", "Unit"));
        }

        public static void AddTimerColumns(this CsvLayout layout)
        {
            if (layout == null) throw new ArgumentNullException("layout");
            AddMeterColumns(layout);
            AddHistogramColumns(layout);
        }
    }
}
