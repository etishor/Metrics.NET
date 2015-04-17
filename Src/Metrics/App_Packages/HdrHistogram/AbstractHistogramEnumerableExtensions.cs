using System.Collections.Generic;

namespace HdrHistogram
{
    public static class AbstractHistogramEnumerableExtensions
    {
        private static IEnumerable<HistogramIterationValue> IterateOver(AbstractHistogramIterator iterator)
        {
            using (iterator)
            {
                while (iterator.MoveNext())
                {
                    yield return iterator.Current;
                }
            }
        }

        /// <summary>
        /// Provide a means of iterating through histogram values according to percentile levels. The iteration is
        /// performed in steps that start at 0% and reduce their distance to 100% according to the
        /// <i>percentileTicksPerHalfDistance</i> parameter, ultimately reaching 100% when all recorded histogram
        /// values are exhausted. 
        /// <seealso cref="PercentileIterator"/>
        /// </summary>
        /// <param name="histogram">The histogram on which to iterate.</param>
        /// <param name="percentileTicksPerHalfDistance">The number of iteration steps per half-distance to 100%.</param>
        /// <returns></returns>
        public static IEnumerable<HistogramIterationValue> Percentiles(this AbstractHistogram histogram, int percentileTicksPerHalfDistance)
        {
            return IterateOver(new PercentileIterator(histogram, percentileTicksPerHalfDistance));
        }

        /// <summary>
        /// Provide a means of iterating through histogram values using linear steps. The iteration is
        /// performed in steps of <i>valueUnitsPerBucket</i> in size, terminating when all recorded histogram
        /// values are exhausted.
        /// <seealso cref="LinearIterator"/>
        /// </summary>
        /// <param name="histogram">The histogram on which to iterate.</param>
        /// <param name="valueUnitsPerBucket">The size (in value units) of the linear buckets to use</param>
        /// <returns></returns>
        public static IEnumerable<HistogramIterationValue> LinearBucketValues(this AbstractHistogram histogram, long valueUnitsPerBucket)
        {
            return IterateOver(new LinearIterator(histogram, valueUnitsPerBucket));
        }

        /// <summary>
        /// Provide a means of iterating through histogram values at logarithmically increasing levels. The iteration is
        /// performed in steps that start at <i>valueUnitsInFirstBucket</i> and increase exponentially according to
        /// <i>logBase</i>, terminating when all recorded histogram values are exhausted.
        /// <seealso cref="LogarithmicIterator"/>
        /// </summary>
        /// <param name="histogram">The histogram on which to iterate.</param>
        /// <param name="valueUnitsInFirstBucket">The size (in value units) of the first bucket in the iteration.</param>
        /// <param name="logBase">The multiplier by which bucket sizes will grow in each iteration step.</param>
        /// <returns></returns>
        public static IEnumerable<HistogramIterationValue> LogarithmicBucketValues(this AbstractHistogram histogram, long valueUnitsInFirstBucket, double logBase)
        {
            return IterateOver(new LogarithmicIterator(histogram, valueUnitsInFirstBucket, logBase));
        }

        /// <summary>
        /// Provide a means of iterating through all recorded histogram values using the finest granularity steps
        /// supported by the underlying representation. The iteration steps through all non-zero recorded value counts,
        /// and terminates when all recorded histogram values are exhausted.
        /// <seealso cref="RecordedValuesIterator"/>
        /// </summary>
        /// <param name="histogram">The histogram on which to iterate.</param>
        /// <returns></returns>
        public static IEnumerable<HistogramIterationValue> RecordedValues(this AbstractHistogram histogram)
        {
            return IterateOver(new RecordedValuesIterator(histogram));
        }

        /// <summary>
        /// Provide a means of iterating through all histogram values using the finest granularity steps supported by
        /// the underlying representation. The iteration steps through all possible unit value levels, regardless of
        /// whether or not there were recorded values for that value level, and terminates when all recorded histogram
        /// values are exhausted.
        /// <seealso cref="AllValuesIterator"/>
        /// </summary>
        /// <param name="histogram">The histogram on which to iterate.</param>
        /// <returns></returns>
        public static IEnumerable<HistogramIterationValue> AllValues(this AbstractHistogram histogram)
        {
            return IterateOver(new AllValuesIterator(histogram));
        }
    }
}
