using System;

namespace Metrics.Reporters
{
    public class ConsoleReport : HumanReadableReport
    {
        protected override void StartReport(string contextName, DateTime timestamp)
        {
            Console.Clear();
            base.StartReport(contextName, timestamp);
        }

        protected override void WriteLine(string line, params string[] args)
        {
            Console.WriteLine(string.Format(line, args));
        }
    }
}
