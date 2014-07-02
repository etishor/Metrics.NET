using System;
using System.Collections.Generic;

namespace Metrics.Reporters
{
  public interface ICSVFileAppender
  {
    void AppendLine(DateTime timestamp, string metricType, string metricName, IEnumerable<CSVReporter.Value> values);
  }
}