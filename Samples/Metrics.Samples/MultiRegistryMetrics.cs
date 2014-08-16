using System;
using System.Linq;
using Metrics.Core;
using Metrics.Reporters;

namespace Metrics.Samples
{
    public class MultiRegistryMetrics
    {
        private readonly MetricsRegistry first = new LocalRegistry("First Registry");
        private readonly MetricsRegistry second = new LocalRegistry("Second Registry");

        private readonly Counter firstCounter;
        private readonly Meter secondMeter;

        public MultiRegistryMetrics()
        {
            this.firstCounter = first.Counter("Counter In First Registry", Unit.Requests);
            this.secondMeter = second.Meter("Meter In Second Registry", Unit.Errors, TimeUnit.Seconds);
        }

        public void Run()
        {
            this.firstCounter.Increment();
            this.secondMeter.Mark();
        }

        public void Report()
        {
            var jsonFirst = RegistrySerializer.GetAsJson(this.first);
            var jsonSecond = RegistrySerializer.GetAsJson(this.second);

            var countersFromFirst = this.first.Counters.Select(c => new { Name = c.Name, Value = c.Value })
                .ToArray();

            Console.WriteLine(jsonFirst);
            Console.WriteLine(jsonSecond);

            Console.WriteLine(countersFromFirst.Length);
        }
    }
}
