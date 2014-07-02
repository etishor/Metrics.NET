using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Metrics.Reporters;
using log4net;

namespace Metrics.RollingCsvReporter
{
 
  public class RollingCsvFileAppender: ICSVFileAppender
  {
    private readonly string directory;
    private readonly Func<string, string, string, ILog> getLogger;
    private readonly string delimiter;

    public RollingCsvFileAppender(string directory, Func<string, string, string, ILog> getLogger, string delimiter)
    {
      this.directory = directory;
      this.getLogger = getLogger;
      this.delimiter = delimiter;
    }

    public void AppendLine(DateTime timestamp, string metricType, string metricName, IEnumerable<CSVReporter.Value> values)
    {
      var name = string.Format("{0}.{1}.csv", metricName, metricType);

      var fileName = Path.Combine(this.directory, CleanFileName(name));

      var logger = getLogger(metricName + metricType, GetHeader(values), fileName);
      
      logger.Info(GetValues(timestamp, values));
    }

    private string CleanFileName(string name)
    {
      var invalid = Path.GetInvalidFileNameChars();
      foreach (var c in invalid)
      {
        name = name.Replace(c, '_');
      }
      return name;
    }

    private string GetHeader(IEnumerable<CSVReporter.Value> values)
    {
      return string.Join(delimiter, new[] { "Date", "Ticks" }.Concat(values.Select(v => v.Name)));
    }

    private string GetValues(DateTime timestamp, IEnumerable<CSVReporter.Value> values)
    {
      return string.Join(delimiter, new[] { timestamp.ToString(), timestamp.Ticks.ToString("D") }.Concat(values.Select(v => v.FormattedValue)));
    }
  }
}
