﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Metrics.Reporters
{
    public class ConsoleCSVAppender : CSVAppender
    {
        public ConsoleCSVAppender(string delimiter = CSVAppender.CommaDelimiter) : base(delimiter) { }

        public override void AppendLine(DateTime timestamp, string metricType, string metricName, IEnumerable<CSVReporter.Value> values)
        {
            Console.WriteLine(GetHeader(values));
            Console.WriteLine(GetValues(timestamp, values));
        }
    }

    public abstract class CSVAppender
    {
        public const string CommaDelimiter = ",";

        private readonly string delimiter;

        public CSVAppender(string delimiter)
        {
            if (delimiter == null)
            {
                throw new ArgumentNullException("delimiter");
            }

            this.delimiter = delimiter;
        }

        public abstract void AppendLine(DateTime timestamp, string metricType, string metricName, IEnumerable<CSVReporter.Value> values);

        protected virtual string GetHeader(IEnumerable<CSVReporter.Value> values)
        {
            return string.Join(this.delimiter, new[] { "Date", "Ticks" }.Concat(values.Select(v => v.Name)));
        }

        protected virtual string GetValues(DateTime timestamp, IEnumerable<CSVReporter.Value> values)
        {
            return string.Join(this.delimiter, new[] { timestamp.ToString("u"), timestamp.Ticks.ToString("D") }.Concat(values.Select(v => v.FormattedValue)));
        }
    }
}
