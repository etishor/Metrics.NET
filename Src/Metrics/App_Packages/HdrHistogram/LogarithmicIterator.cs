// Written by Gil Tene of Azul Systems, and released to the public domain,
// as explained at http://creativecommons.org/publicdomain/zero/1.0/
// 
// Ported to .NET by Iulian Margarintescu under the same license and terms as the java version
// Java Version repo: https://github.com/HdrHistogram/HdrHistogram
// Latest ported version is available in the Java submodule in the root of the repo
namespace HdrHistogram
{
    /**
 * Written by Gil Tene of Azul Systems, and released to the public domain,
 * as explained at http://creativecommons.org/publicdomain/zero/1.0/
 *
 * @author Gil Tene
 */

    /**
     * Used for iterating through histogram values in logarithmically increasing levels. The iteration is
     * performed in steps that start at <i>valueUnitsInFirstBucket</i> and increase exponentially according to
     * <i>logBase</i>, terminating when all recorded histogram values are exhausted. Note that each iteration "bucket"
     * includes values up to and including the next bucket boundary value.
     */
    public class LogarithmicIterator : AbstractHistogramIterator
    {
        long valueUnitsInFirstBucket;
        double logBase;
        long nextValueReportingLevel;
        long nextValueReportingLevelLowestEquivalent;

        /**
         * Reset iterator for re-use in a fresh iteration over the same histogram data set.
         * @param valueUnitsInFirstBucket the size (in value units) of the first value bucket step
         * @param logBase the multiplier by which the bucket size is expanded in each iteration step.
         */
        public void reset(long valueUnitsInFirstBucket, double logBase)
        {
            reset(histogram, valueUnitsInFirstBucket, logBase);
        }

        private void reset(AbstractHistogram histogram, long valueUnitsInFirstBucket, double logBase)
        {
            base.resetIterator(histogram);
            this.logBase = logBase;
            this.valueUnitsInFirstBucket = valueUnitsInFirstBucket;
            this.nextValueReportingLevel = valueUnitsInFirstBucket;
            this.nextValueReportingLevelLowestEquivalent = histogram.lowestEquivalentValue(nextValueReportingLevel);
        }

        /**
         * @param histogram The histogram this iterator will operate on
         * @param valueUnitsInFirstBucket the size (in value units) of the first value bucket step
         * @param logBase the multiplier by which the bucket size is expanded in each iteration step.
         */
        public LogarithmicIterator(AbstractHistogram histogram, long valueUnitsInFirstBucket, double logBase)
        {
            reset(histogram, valueUnitsInFirstBucket, logBase);
        }

        public override bool hasNext()
        {
            if (base.hasNext())
            {
                return true;
            }
            // If next iterate does not move to the next sub bucket index (which is empty if
            // if we reached this point), then we are not done iterating... Otherwise we're done.
            return (nextValueReportingLevelLowestEquivalent < nextValueAtIndex);
        }

        protected override void incrementIterationLevel()
        {
            // TODO: check conversion 
            nextValueReportingLevel = (long)(nextValueAtIndex * logBase);
            nextValueReportingLevelLowestEquivalent = histogram.lowestEquivalentValue(nextValueReportingLevel);
        }

        long getValueIteratedTo()
        {
            return nextValueReportingLevel;
        }


        protected override bool reachedIterationLevel()
        {
            return (currentValueAtIndex >= nextValueReportingLevelLowestEquivalent);
        }
    }

}
