using System.Threading;

namespace Metrics.StupidBenchmarks
{
    public class WorkLoad
    {
        private static readonly Timer timer = Metric.Timer("test", Unit.Calls, durationUnit: TimeUnit.Nanoseconds);

        public void DoSomeWork()
        {
            Thread.SpinWait(10000);
        }

        public void DoSomeWorkWithATimer()
        {
            using (timer.NewContext())
            {
                DoSomeWork();
            }
        }
    }
}
