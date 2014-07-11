namespace Metrics.Log4Net.Layout
{
    public class CsvColumn
    {
        public string ColumnName { get; set; }

        public string LoggingEventPropertyKey { get; set; }

        public CsvColumn(string columnName, string propertyKey)
        {
            ColumnName = columnName;
            LoggingEventPropertyKey = propertyKey;
        }
    }
}