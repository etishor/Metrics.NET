
namespace Metrics.Samples
{
    public class UserValueHistogramSample
    {
        private readonly Histogram histogram =
            Metric.Histogram("Results", Unit.Items);

        public void Process(string documentId)
        {
            var results = GetResultsForDocument(documentId);
            this.histogram.Update(results.Length, documentId);
        }

        private int[] GetResultsForDocument(string documentId)
        {
            return new int[ThreadLocalRandom.NextLong() % 100];
        }

        public static void RunSomeRequests()
        {
            for (int i = 0; i < 30; i++)
            {
                var documentId = ThreadLocalRandom.NextLong() % 10;
                new UserValueHistogramSample().Process("document-" + documentId.ToString());
            }
        }
    }
}
