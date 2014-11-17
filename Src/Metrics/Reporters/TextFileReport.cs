using System;
using System.IO;
using System.Text;

namespace Metrics.Reporters
{
    public class TextFileReport : HumanReadableReport
    {
        private readonly string fileName;

        private StringBuilder buffer = null;

        public TextFileReport(string fileName)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            this.fileName = fileName;
        }

        protected override void StartReport(string contextName, DateTime timestamp)
        {
            this.buffer = new StringBuilder();
            base.StartReport(contextName, timestamp);
        }

        protected override void WriteLine(string line, params string[] args)
        {
            buffer.AppendFormat(line, args);
            buffer.AppendLine();
        }

        protected override void EndReport(string contextName, DateTime timestamp)
        {
            try
            {
                File.WriteAllText(this.fileName, this.buffer.ToString());
            }
            catch (Exception x)
            {
                MetricsErrorHandler.Handle(x, "Error writing text file " + this.fileName);
            }

            base.EndReport(contextName, timestamp);
            this.buffer = null;
        }
    }
}
