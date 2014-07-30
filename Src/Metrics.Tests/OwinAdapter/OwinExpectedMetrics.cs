namespace Metrics.Tests.OwinAdapter
{
    public class OwinExpectedMetrics
    {
        public int TimePerRequestMilliseconds { get; private set; }
        public int ErrorCount { get; private set; }
        public int RequestCount { get; private set; }
        public int TimerRateCount { get { return RequestCount; } }
        public int HistogramCount { get { return RequestCount; } }
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