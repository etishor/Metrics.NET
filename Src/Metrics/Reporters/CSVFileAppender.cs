using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Metrics.Reporters
{
    public class CSVFileAppender
    {
        private readonly string directory;

        public CSVFileAppender(string directory)
        {
            this.directory = directory;
        }

        public virtual void AppendLine(DateTime timestamp, string metricType, string metricName, IEnumerable<CSVReporter.Value> values)
        {
            var fileName = Path.Combine(this.directory, string.Format("{0}.{1}.csv", metricName, metricType));
            if (!File.Exists(fileName))
            {
                File.AppendAllLines(fileName, new[] { GetHeader(values), GetValues(timestamp, values) });
            }
            else
            {
                File.AppendAllLines(fileName, new[] { GetValues(timestamp, values) });
            }
        }

        protected static string GetHeader(IEnumerable<CSVReporter.Value> values)
        {
            return string.Join(",", new[] { "Date", "Ticks" }.Concat(values.Select(v => v.Name)));
        }

        protected static string GetValues(DateTime timestamp, IEnumerable<CSVReporter.Value> values)
        {
            return string.Join(",", new[] { timestamp.ToString(), timestamp.Ticks.ToString("D") }.Concat(values.Select(v => v.FormattedValue)));
        }
    }

    public class ConsoleCSVAppender : CSVFileAppender
    {
        public ConsoleCSVAppender() : base(null) { }

        public override void AppendLine(DateTime timestamp, string metricType, string metricName, IEnumerable<CSVReporter.Value> values)
        {
            Console.WriteLine(GetHeader(values));
            Console.WriteLine(GetValues(timestamp, values));
        }
    }
}
