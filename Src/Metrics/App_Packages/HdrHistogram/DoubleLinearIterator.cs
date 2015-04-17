// Written by Gil Tene of Azul Systems, and released to the public domain,
// as explained at http://creativecommons.org/publicdomain/zero/1.0/
// 
// Ported to .NET by Iulian Margarintescu under the same license and terms as the java version
// Java Version repo: https://github.com/HdrHistogram/HdrHistogram
// Latest ported version is available in the Java submodule in the root of the repo


namespace HdrHistogram
{

    /**
     * Used for iterating through {@link DoubleHistogram} values in linear steps. The iteration is
     * performed in steps of <i>valueUnitsPerBucket</i> in size, terminating when all recorded histogram
     * values are exhausted. Note that each iteration "bucket" includes values up to and including
     * the next bucket boundary value.
     */

    public class DoubleLinearIterator : Iterator<DoubleHistogramIterationValue>
    {
        private LinearIterator integerLinearIterator;
        private DoubleHistogramIterationValue iterationValue;
        private DoubleHistogram histogram;

        /**
         * Reset iterator for re-use in a fresh iteration over the same histogram data set.
         * @param valueUnitsPerBucket The size (in value units) of each bucket iteration.
         */

        public void reset(double valueUnitsPerBucket)
        {
            integerLinearIterator.reset((long)(valueUnitsPerBucket * histogram.doubleToIntegerValueConversionRatio.GetValue()));
        }

        /**
         * @param histogram The histogram this iterator will operate on
         * @param valueUnitsPerBucket The size (in value units) of each bucket iteration.
         */

        public DoubleLinearIterator(DoubleHistogram histogram, double valueUnitsPerBucket)
        {
            this.histogram = histogram;
            integerLinearIterator = new LinearIterator(
                histogram.integerValuesHistogram,
                (long)(valueUnitsPerBucket * histogram.doubleToIntegerValueConversionRatio.GetValue())
                );
            iterationValue = new DoubleHistogramIterationValue(integerLinearIterator.currentIterationValue);
        }

        public override bool hasNext()
        {
            return integerLinearIterator.hasNext();
        }

        public override DoubleHistogramIterationValue next()
        {
            integerLinearIterator.next();
            return iterationValue;
        }
    }
}
