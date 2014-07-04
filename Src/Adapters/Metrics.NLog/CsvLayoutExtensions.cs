using System.Linq;
using NLog.Layouts;

namespace Metrics.NLog
{
    public static class CsvLayoutExtensions
    {
        public static void AddDateColumns(this CsvLayout layout)
        {
            if (!layout.Columns.Any(l => l.Name == "Date"))
            {
                layout.Columns.Add(new CsvColumn("Date", new SimpleLayout("${event-context:item=Date}")));
            }
            if (!layout.Columns.Any(l => l.Name == "Ticks"))
            {
                layout.Columns.Add(new CsvColumn("Ticks", new SimpleLayout("${event-context:item=Ticks}")));
            }
        }

        public static void AddGaugeColumns(this CsvLayout layout)
        {
            AddDateColumns(layout);
            layout.Columns.Add(new CsvColumn("Value", new SimpleLayout("${event-context:item=Value}")));
            layout.Columns.Add(new CsvColumn("Unit", new SimpleLayout("${event-context:item=Unit}")));
        }

        public static void AddCounterColumns(this CsvLayout layout)
        {
            AddDateColumns(layout);
            layout.Columns.Add(new CsvColumn("Count", new SimpleLayout("${event-context:item=Count}")));
            layout.Columns.Add(new CsvColumn("Unit", new SimpleLayout("${event-context:item=Unit}")));
        }

        public static void AddMeterColumns(this CsvLayout layout)
        {
            AddDateColumns(layout);
            layout.Columns.Add(new CsvColumn("Count", new SimpleLayout("${event-context:item=Count}")));
            layout.Columns.Add(new CsvColumn("Mean Rate", new SimpleLayout("${event-context:item=Mean Rate}")));
            layout.Columns.Add(new CsvColumn("One Minute Rate", new SimpleLayout("${event-context:item=One Minute Rate}")));
            layout.Columns.Add(new CsvColumn("Five Minute Rate", new SimpleLayout("${event-context:item=Five Minute Rate}")));
            layout.Columns.Add(new CsvColumn("Fifteen Minute Rate", new SimpleLayout("${event-context:item=Fifteen Minute Rate}")));
            layout.Columns.Add(new CsvColumn("Rate Unit", new SimpleLayout("${event-context:item=Rate Unit}")));
        }

        public static void AddHistogramColumns(this CsvLayout layout)
        {
            AddDateColumns(layout);
            layout.Columns.Add(new CsvColumn("Last", new SimpleLayout("${event-context:item=Last}")));
            layout.Columns.Add(new CsvColumn("Min", new SimpleLayout("${event-context:item=Min}")));
            layout.Columns.Add(new CsvColumn("Max", new SimpleLayout("${event-context:item=Unit}")));
            layout.Columns.Add(new CsvColumn("Mean", new SimpleLayout("${event-context:item=Mean}")));
            layout.Columns.Add(new CsvColumn("StdDev", new SimpleLayout("${event-context:item=StdDev}")));
            layout.Columns.Add(new CsvColumn("Median", new SimpleLayout("${event-context:item=Median}")));
            layout.Columns.Add(new CsvColumn("75%", new SimpleLayout("${event-context:item=75%}")));
            layout.Columns.Add(new CsvColumn("95%", new SimpleLayout("${event-context:item=95%}")));
            layout.Columns.Add(new CsvColumn("98%", new SimpleLayout("${event-context:item=98%}")));
            layout.Columns.Add(new CsvColumn("99%", new SimpleLayout("${event-context:item=99%}")));
            layout.Columns.Add(new CsvColumn("99.9%", new SimpleLayout("${event-context:item=99.9%}")));
            layout.Columns.Add(new CsvColumn("Unit", new SimpleLayout("${event-context:item=Unit}")));
        }

        public static void AddTimerColumns(this CsvLayout layout)
        {
            AddMeterColumns(layout);
            AddHistogramColumns(layout);
        }
    }
}
