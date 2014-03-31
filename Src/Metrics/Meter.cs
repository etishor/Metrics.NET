using Metrics.Utils;

namespace Metrics
{
    /// <summary>
    /// A meter measures the rate at which a set of events occur, in a few different ways. 
    /// The mean rate is the average rate of events. It’s generally useful for trivia, 
    /// but as it represents the total rate for your application’s entire lifetime (e.g., the total number of requests handled, 
    /// divided by the number of seconds the process has been running), it doesn’t offer a sense of recency. 
    /// Luckily, meters also record three different exponentially-weighted moving average rates: the 1-, 5-, and 15-minute moving averages.
    /// </summary>
    public interface Meter : Metric<MeterValue>
    {
        /// <summary>
        /// Mark the occurrence of an event.
        /// </summary>
        void Mark();

        /// <summary>
        /// Mark the occurrence of <paramref name="count"/> events.
        /// </summary>
        /// <param name="count"></param>
        void Mark(long count);
    }

    /// <summary>
    /// The value reported by a Meter Metric
    /// </summary>
    public struct MeterValue
    {
        public readonly long Count;
        public readonly double MeanRate;
        public readonly double OneMinuteRate;
        public readonly double FiveMinuteRate;
        public readonly double FifteenMinuteRate;

        public MeterValue(long count, double meanRate, double oneMinuteRate, double fiveMinuteRate, double fifteenMinuteRate)
        {
            this.Count = count;
            this.MeanRate = meanRate;
            this.OneMinuteRate = oneMinuteRate;
            this.FiveMinuteRate = fiveMinuteRate;
            this.FifteenMinuteRate = fifteenMinuteRate;
        }

        public MeterValue Scale(TimeUnit unit)
        {
            var factor = unit.ToSeconds(1);
            return new MeterValue(this.Count, this.MeanRate * factor, this.OneMinuteRate * factor, this.FiveMinuteRate * factor, this.FifteenMinuteRate * factor);
        }
    }

}
