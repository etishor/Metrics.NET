using System.Collections.Generic;
using System.IO;

namespace Metrics.Reporters
{
    public class TextFileReporter : HumanReadableReporter
    {
        private readonly string fileName;
        private readonly List<string> buffer = new List<string>();

        public TextFileReporter(string fileName)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            this.fileName = fileName;
        }

        protected override void WriteLine(string line, params string[] args)
        {
            this.buffer.Add(string.Format(line, args));
        }

        protected override void EndReport()
        {
            base.EndReport();
            File.WriteAllLines(this.fileName, this.buffer);
            buffer.Clear();
        }
    }
}
