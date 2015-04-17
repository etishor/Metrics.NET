// Written by Gil Tene of Azul Systems, and released to the public domain,
// as explained at http://creativecommons.org/publicdomain/zero/1.0/
// 
// Ported to .NET by Iulian Margarintescu under the same license and terms as the java version
// Java Version repo: https://github.com/HdrHistogram/HdrHistogram
// Latest ported version is available in the Java submodule in the root of the repo

namespace HdrHistogram
{

    /**
     * Used for iterating through {@link DoubleHistogram} values values according to percentile levels. The iteration is
     * performed in steps that start at 0% and reduce their distance to 100% according to the
     * <i>percentileTicksPerHalfDistance</i> parameter, ultimately reaching 100% when all recorded histogram
     * values are exhausted.
     */

    public class DoublePercentileIterator : Iterator<DoubleHistogramIterationValue>
    {
        private PercentileIterator integerPercentileIterator;
        private DoubleHistogramIterationValue iterationValue;
        private DoubleHistogram histogram;

        /**
     * Reset iterator for re-use in a fresh iteration over the same histogram data set.
     *
     * @param percentileTicksPerHalfDistance The number of iteration steps per half-distance to 100%.
     */

        public void reset(int percentileTicksPerHalfDistance)
        {
            integerPercentileIterator.reset(percentileTicksPerHalfDistance);
        }

        /**
     * @param histogram The histogram this iterator will operate on
     * @param percentileTicksPerHalfDistance The number of iteration steps per half-distance to 100%.
     */

        public DoublePercentileIterator(DoubleHistogram histogram, int percentileTicksPerHalfDistance)
        {
            this.histogram = histogram;
            integerPercentileIterator = new PercentileIterator(
                histogram.integerValuesHistogram,
                percentileTicksPerHalfDistance
                );
            iterationValue = new DoubleHistogramIterationValue(integerPercentileIterator.currentIterationValue);
        }

        public override bool hasNext()
        {
            return integerPercentileIterator.hasNext();
        }

        public override DoubleHistogramIterationValue next()
        {
            integerPercentileIterator.next();
            return iterationValue;
        }
    }
}
