// Written by Gil Tene of Azul Systems, and released to the public domain,
// as explained at http://creativecommons.org/publicdomain/zero/1.0/
// 
// Ported to .NET by Iulian Margarintescu under the same license and terms as the java version
// Java Version repo: https://github.com/HdrHistogram/HdrHistogram
// Latest ported version is available in the Java submodule in the root of the repo

namespace HdrHistogram
{

    /**
     * Used for iterating through {@link DoubleHistogram} values using the finest granularity steps supported by the
     * underlying representation. The iteration steps through all possible unit value levels, regardless of whether or not
     * there were recorded values for that value level, and terminates when all recorded histogram values are exhausted.
     */

    public class DoubleAllValuesIterator : Iterator<DoubleHistogramIterationValue>
    {
        private AllValuesIterator integerAllValuesIterator;
        private DoubleHistogramIterationValue iterationValue;
        private DoubleHistogram histogram;

        /**
     * Reset iterator for re-use in a fresh iteration over the same histogram data set.
     */

        public void reset()
        {
            integerAllValuesIterator.reset();
        }

        /**
     * @param histogram The histogram this iterator will operate on
     */

        public DoubleAllValuesIterator(DoubleHistogram histogram)
        {
            this.histogram = histogram;
            integerAllValuesIterator = new AllValuesIterator(histogram.integerValuesHistogram);
            iterationValue = new DoubleHistogramIterationValue(integerAllValuesIterator.currentIterationValue);
        }

        public override bool hasNext()
        {
            return integerAllValuesIterator.hasNext();
        }

        public override DoubleHistogramIterationValue next()
        {
            integerAllValuesIterator.next();
            return iterationValue;
        }
    }
}
