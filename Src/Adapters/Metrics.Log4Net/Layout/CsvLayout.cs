using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Metrics.Log4Net.Layout
{
    public abstract class CsvLayout : log4net.Layout.SimpleLayout
    {
        public ICollection<CsvColumn> Columns { get; private set; }

        protected CsvLayout()
        {
            Columns = new Collection<CsvColumn>();
        }

        public override string Header
        {
            get { return CreateHeader(); }
        }

        private string CreateHeader()
        {
            return string.Join(CsvDelimiter.Value, Columns.Select(x => x.ColumnName)) + Environment.NewLine;
        }

        public override void Format(TextWriter writer, log4net.Core.LoggingEvent loggingEvent)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            if (loggingEvent == null) throw new ArgumentNullException("loggingEvent");

            var lineBuilder = new StringBuilder();
            foreach (var col in Columns)
            {
                var value = loggingEvent.LookupProperty(col.LoggingEventPropertyKey);
                lineBuilder.AppendFormat("{0}{1}", value, CsvDelimiter.Value);
            }
            writer.WriteLineAsync(lineBuilder.ToString());
        }
    }
}