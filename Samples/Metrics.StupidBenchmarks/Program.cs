
using System;
using Metrics.Core;
using Metrics.Utils;
namespace Metrics.StupidBenchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            Clock.TestClock clock = new Clock.TestClock();
            Timer timer = new TimerMetric(SamplingType.FavourRecent, clock);

            //Counter counter = new CounterMetric();
            //Meter meter = new MeterMetric(clock);

            for (int i = 1; i < 16; i++)
            {
                //TestMeter(clock, meter, i);
                //TestCounter(counter, i);
                TestTimer(clock, timer, i);
            }

            Console.WriteLine("done");
            Console.ReadKey();
        }

        private static void TestMeter(Clock.TestClock clock, Meter meter, int threadCount)
        {
            var meterResult = Benchmark.Run((i, l) => { meter.Mark(i); clock.Advance(TimeUnit.Milliseconds, l); },
                iterations: 50000, threadCount: threadCount);
            Console.WriteLine("{0}\t{1}", threadCount, meterResult.PerSecond);
        }

        private static void TestCounter(Counter counter, int threadCount)
        {
            var counterResult = Benchmark.Run((i, l) => counter.Increment(l), iterations: 100000, threadCount: threadCount);
            Console.WriteLine("{0}\t{1}", threadCount, counterResult.PerSecond);
        }

        private static void TestTimer(Clock.TestClock clock, Timer timer, int threadCount)
        {
            var timerResult = Benchmark.Run((i, l) => { using (timer.NewContext()) { clock.Advance(TimeUnit.Milliseconds, l); } },
                iterations: 100000, threadCount: threadCount);
            Console.WriteLine("{0}\t{1}", threadCount, timerResult.PerSecond);
        }
    }
}
