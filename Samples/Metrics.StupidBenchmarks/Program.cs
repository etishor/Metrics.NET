using Metrics.Core;
namespace Metrics.StupidBenchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            //FixedTimeBenchmark.Run<CounterMetric>(m => m.Increment());
            //FixedTimeBenchmark.Run<MeterMetric>(m => m.Mark());
            //FixedTimeBenchmark.Run<HistogramMetric>(m => m.Update(1));
            FixedTimeBenchmark.Run<TimerMetric>(m => { using (m.NewContext()) { } }, maxThreads: 32, seconds: 8);

            //FixedIterationsBenchmark.Run<TimerMetric>(m => { using (m.NewContext()) { } }, 1000 * 1000);
        }
    }
}
