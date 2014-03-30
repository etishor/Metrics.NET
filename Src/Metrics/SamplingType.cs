
namespace Metrics
{
    public enum SamplingType
    {
        /// <summary>
        /// Sampling will be done with a Exponentially Decaying Reservoir.
        /// <remarks>
        /// A histogram with an exponentially decaying reservoir produces quantiles which are representative of (roughly) the last five minutes of data.
        /// It does so by using a forward-decaying priority reservoir with an exponential weighting towards newer data. 
        /// Unlike the uniform reservoir, an exponentially decaying reservoir represents recent data, allowing you to know very quickly if the distribution 
        /// of the data has changed.
        /// </remarks>
        /// <seealso cref="http://metrics.codahale.com/manual/core/#man-core-histograms"/>
        /// </summary>
        FavourRecent,
        /// <summary>
        /// Sampling will done with a Uniform Reservoir.
        /// <remarks>
        /// A histogram with a uniform reservoir produces quantiles which are valid for the entirely of the histogram’s lifetime.
        /// It will return a median value, for example, which is the median of all the values the histogram has ever been updated with.
        /// Use a uniform histogram when you’re interested in long-term measurements. 
        /// Don’t use one where you’d want to know if the distribution of the underlying data stream has changed recently.
        /// </remarks>
        /// <seealso cref="http://metrics.codahale.com/manual/core/#man-core-histograms"/>
        /// </summary>
        LongTerm,
        /// <summary>
        /// Sampling will done with a Sliding Window Reservoir.
        /// A histogram with a sliding window reservoir produces quantiles which are representative of the past N measurements.
        /// <seealso cref="http://metrics.codahale.com/manual/core/#man-core-histograms"/>
        /// </summary>
        SlidingWindow
    }
}
