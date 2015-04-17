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
     * Used for iterating through histogram values in linear steps. The iteration is
     * performed in steps of <i>valueUnitsPerBucket</i> in size, terminating when all recorded histogram
     * values are exhausted. Note that each iteration "bucket" includes values up to and including
     * the next bucket boundary value.
     */
    public class LinearIterator : AbstractHistogramIterator
    {
        long valueUnitsPerBucket;
        long nextValueReportingLevel;
        long nextValueReportingLevelLowestEquivalent;

        /**
         * Reset iterator for re-use in a fresh iteration over the same histogram data set.
         * @param valueUnitsPerBucket The size (in value units) of each bucket iteration.
         */
        public void reset(long valueUnitsPerBucket)
        {
            reset(histogram, valueUnitsPerBucket);
        }

        private void reset(AbstractHistogram histogram, long valueUnitsPerBucket)
        {
            base.resetIterator(histogram);
            this.valueUnitsPerBucket = valueUnitsPerBucket;
            this.nextValueReportingLevel = valueUnitsPerBucket;
            this.nextValueReportingLevelLowestEquivalent = histogram.lowestEquivalentValue(nextValueReportingLevel);
        }

        /**
         * @param histogram The histogram this iterator will operate on
         * @param valueUnitsPerBucket The size (in value units) of each bucket iteration.
         */
        public LinearIterator(AbstractHistogram histogram, long valueUnitsPerBucket)
        {
            reset(histogram, valueUnitsPerBucket);
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
            nextValueReportingLevel += valueUnitsPerBucket;
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
