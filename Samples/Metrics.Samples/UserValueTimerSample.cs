
using System;
using System.Threading;
using Metrics.Utils;
namespace Metrics.Samples
{
    public class UserValueTimerSample
    {
        private readonly Timer timer =
            Metric.Timer("Requests", Unit.Requests);

        public void Process(string documentId)
        {
            using (var context = timer.NewContext(documentId))
            {
                ActualProcessingOfTheRequest(documentId);

                // if needed elapsed time is available in context.Elapsed 
            }
        }

        private void LogDuration(TimeSpan time)
        {
        }

        private void ActualProcessingOfTheRequest(string documentId)
        {
            Thread.Sleep((int)ThreadLocalRandom.NextLong() % 1000);
        }

        public static void RunSomeRequests()
        {
            for (int i = 0; i < 30; i++)
            {
                var documentId = ThreadLocalRandom.NextLong() % 10;
                new UserValueTimerSample().Process("document-" + documentId.ToString());
            }
        }
    }
}
