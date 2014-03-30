using System;

namespace Metrics.Reporters
{
    public class ConsoleReporter : HumanReadableReporter
    {
        protected override void StartReport()
        {
            Console.Clear();
            base.StartReport();
        }

        protected override void WriteLine(string line, params string[] args)
        {
            Console.WriteLine(string.Format(line, args));
        }
    }
}
