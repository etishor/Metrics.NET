namespace Metrics.Tests.OwinAdapter
{
    public class OwinExpectedMetrics
    {
        public int TimePerRequestMilliseconds { get; private set; }
        public int ErrorCount { get; private set; }
        public int RequestCount { get; private set; }

        // the test registry reuses the same timer instance 
        // expect (2 * actual requests) timer requests - 2 for each request, 
        // one for the global metric for all requests and one for the metric for each request

        public int TimerRateCount { get { return RequestCount * 2; } }

        public int HistogramCount { get { return RequestCount * 2; } }
        public int TotalExecutionTime { get { return TimePerRequestMilliseconds * RequestCount; } }
        public long HistogramAverageLower { get { return TotalExecutionTime / HistogramCount; } }
        public long HistogramAverageUpper { get { return TimePerRequestMilliseconds * (RequestCount + 1) / HistogramCount; } }


        public OwinExpectedMetrics(int timePerRequestMilliseconds, int numberOfRequests, int errors)
        {
            TimePerRequestMilliseconds = timePerRequestMilliseconds;
            RequestCount = numberOfRequests;
            ErrorCount = errors;
        }
    }
}