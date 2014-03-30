using System.Text;

namespace Metrics.Reporters
{
    public class StringReporter : HumanReadableReporter
    {
        private readonly StringBuilder buffer = new StringBuilder();

        protected override void WriteLine(string line, params string[] args)
        {
            this.buffer.AppendLine(string.Format(line, args));
        }

        public string Result { get { return this.buffer.ToString(); } }
    }
}
