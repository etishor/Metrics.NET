using System;
using System.Collections.Generic;
using System.IO;

namespace Metrics.Reporters
{
    public class TextFileReport : HumanReadableReport
    {
        private readonly string fileName;
        private readonly List<string> buffer = new List<string>();

        public TextFileReport(string fileName)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            this.fileName = fileName;
        }

        protected override void WriteLine(string line, params string[] args)
        {
            this.buffer.Add(string.Format(line, args));
        }

        protected override void EndReport(string contextName, DateTime timestamp)
        {
            File.WriteAllLines(this.fileName, this.buffer);
            buffer.Clear();
            base.EndReport(contextName, timestamp);
        }
    }
}
