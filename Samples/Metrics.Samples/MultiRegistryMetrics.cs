
namespace Metrics.Samples
{
    public class MultiRegistryMetrics
    {
        private readonly Counter firstCounter = Metric.Context("First Context").Counter("Counter", Unit.Requests);

        private readonly Counter secondCounter = Metric.Context("Second Context").Counter("Counter", Unit.Requests);
        private readonly Meter secondMeter = Metric.Context("Second Context").Meter("Meter", Unit.Errors, TimeUnit.Seconds);

        public void Run()
        {
            this.firstCounter.Increment();
            this.secondCounter.Increment();
            this.secondMeter.Mark();
        }
    }
}
