using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace Metrics.Reporters
{
    public class CSVFileAppender
    {
        private readonly string directory;
        private readonly string delimiter;

        public CSVFileAppender(string directory, string delimiter)
        {
            Directory.CreateDirectory(directory);
            this.directory = directory;
            this.delimiter = delimiter;
        }

        protected virtual string FormatFileName(string directory, string metricName, string metricType)
        {
            var name = string.Format("{0}.{1}.csv", metricName, metricType);
            return Path.Combine(directory, CleanFileName(name));
        }

        public virtual void AppendLine(DateTime timestamp, string metricType, string metricName, IEnumerable<CSVReporter.Value> values)
        {
            var fileName = FormatFileName(this.directory, metricName, metricType);

            if (!File.Exists(fileName))
            {
                File.AppendAllLines(fileName, new[] { GetHeader(values), GetValues(timestamp, values) });
            }
            else
            {
                File.AppendAllLines(fileName, new[] { GetValues(timestamp, values) });
            }
        }

        protected virtual string CleanFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            foreach (var c in invalid)
            {
                name = name.Replace(c, '_');
            }
            return name;
        }

        protected virtual string GetHeader(IEnumerable<CSVReporter.Value> values)
        {
            return string.Join(this.delimiter, new[] { "Date", "Ticks" }.Concat(values.Select(v => v.Name)));
        }

        protected virtual string GetValues(DateTime timestamp, IEnumerable<CSVReporter.Value> values)
        {
            return string.Join(this.delimiter, new[] { timestamp.ToString(), timestamp.Ticks.ToString("D") }.Concat(values.Select(v => v.FormattedValue)));
        }
    }

    public class ConsoleCSVAppender : CSVFileAppender
    {
        public ConsoleCSVAppender() : base(null, ",") { }

        public override void AppendLine(DateTime timestamp, string metricType, string metricName, IEnumerable<CSVReporter.Value> values)
        {
            Console.WriteLine(GetHeader(values));
            Console.WriteLine(GetValues(timestamp, values));
        }
    }
}
