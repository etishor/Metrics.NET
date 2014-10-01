using Metrics.Core;
using Metrics.Sampling;
using Metrics.Utils;
namespace Metrics.StupidBenchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            //FixedTimeBenchmark.Run<CounterMetric>(m => m.Increment());
            //FixedTimeBenchmark.Run<MeterMetric>(m => m.Mark());
            //FixedTimeBenchmark.Run<HistogramMetric>(m => m.Update(1));

            FixedTimeBenchmark.Run(() => EWMA.OneMinuteEWMA(), m => m.Update(1), maxThreads: 32, seconds: 5, decrement: 4);

            //FixedTimeBenchmark.Run(() => new TimerMetric(new SlidingWindowReservoir()), m => { using (m.NewContext()) { } }, maxThreads: 32, seconds: 5, decrement: 4);

            //FixedTimeBenchmark.Run(() => new ExponentiallyDecayingReservoir(), r => r.Update(100), maxThreads: 32, seconds: 5, decrement: 4);
            //FixedTimeBenchmark.Run(() => new UniformReservoir(), r => r.Update(100), maxThreads: 32, seconds: 5, decrement: 4);

            //FixedIterationsBenchmark.Run<TimerMetric>(m => { using (m.NewContext()) { } }, 1000 * 1000);
        }
    }
}
