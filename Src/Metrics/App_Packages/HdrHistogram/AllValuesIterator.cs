// Written by Gil Tene of Azul Systems, and released to the public domain,
// as explained at http://creativecommons.org/publicdomain/zero/1.0/
// 
// Ported to .NET by Iulian Margarintescu under the same license and terms as the java version
// Java Version repo: https://github.com/HdrHistogram/HdrHistogram
// Latest ported version is available in the Java submodule in the root of the repo
using System;

namespace HdrHistogram
{
    /**
  * Written by Gil Tene of Azul Systems, and released to the public domain,
  * as explained at http://creativecommons.org/publicdomain/zero/1.0/
  *
  * @author Gil Tene
  */

    /**
     * Used for iterating through histogram values using the finest granularity steps supported by the underlying
     * representation. The iteration steps through all possible unit value levels, regardless of whether or not
     * there were recorded values for that value level, and terminates when all recorded histogram values are exhausted.
     */

    public class AllValuesIterator : AbstractHistogramIterator
    {
        int visitedIndex;

        /**
         * Reset iterator for re-use in a fresh iteration over the same histogram data set.
         */
        public void reset()
        {
            reset(histogram);
        }

        private void reset(AbstractHistogram histogram)
        {
            base.resetIterator(histogram);
            visitedIndex = -1;
        }

        /**
         * @param histogram The histogram this iterator will operate on
         */
        public AllValuesIterator(AbstractHistogram histogram)
        {
            reset(histogram);
        }


        protected override void incrementIterationLevel()
        {
            visitedIndex = currentIndex;
        }


        protected override bool reachedIterationLevel()
        {
            return (visitedIndex != currentIndex);
        }

        public override bool hasNext()
        {
            if (histogram.getTotalCount() != savedHistogramTotalRawCount)
            {
                throw new InvalidOperationException("ConcurrentModificationException");
            }
            // Unlike other iterators AllValuesIterator is only done when we've exhausted the indices:
            return (currentIndex < (histogram.countsArrayLength - 1));
        }
    }

}
