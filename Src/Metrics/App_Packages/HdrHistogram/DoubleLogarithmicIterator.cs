// Written by Gil Tene of Azul Systems, and released to the public domain,
// as explained at http://creativecommons.org/publicdomain/zero/1.0/
// 
// Ported to .NET by Iulian Margarintescu under the same license and terms as the java version
// Java Version repo: https://github.com/HdrHistogram/HdrHistogram
// Latest ported version is available in the Java submodule in the root of the repo

namespace HdrHistogram
{

    /**
     * Used for iterating through {@link DoubleHistogram} values values in logarithmically increasing levels. The
     * iteration is performed in steps that start at <i>valueUnitsInFirstBucket</i> and increase exponentially according to
     * <i>logBase</i>, terminating when all recorded histogram values are exhausted. Note that each iteration "bucket"
     * includes values up to and including the next bucket boundary value.
     */

    public class DoubleLogarithmicIterator : Iterator<DoubleHistogramIterationValue>
    {
        private LogarithmicIterator integerLogarithmicIterator;
        private DoubleHistogramIterationValue iterationValue;
        private DoubleHistogram histogram;

        /**
     * Reset iterator for re-use in a fresh iteration over the same histogram data set.
     * @param valueUnitsInFirstBucket the size (in value units) of the first value bucket step
     * @param logBase the multiplier by which the bucket size is expanded in each iteration step.
     */

        public void reset(double valueUnitsInFirstBucket, double logBase)
        {
            integerLogarithmicIterator.reset(
                (long)(valueUnitsInFirstBucket * histogram.doubleToIntegerValueConversionRatio.GetValue()),
                logBase
                );
        }

        /**
     * @param histogram The histogram this iterator will operate on
     * @param valueUnitsInFirstBucket the size (in value units) of the first value bucket step
     * @param logBase the multiplier by which the bucket size is expanded in each iteration step.
     */

        public DoubleLogarithmicIterator(DoubleHistogram histogram, double valueUnitsInFirstBucket,
            double logBase)
        {
            this.histogram = histogram;
            integerLogarithmicIterator = new LogarithmicIterator(
                histogram.integerValuesHistogram,
                (long)(valueUnitsInFirstBucket * histogram.doubleToIntegerValueConversionRatio.GetValue()),
                logBase
                );
            iterationValue = new DoubleHistogramIterationValue(integerLogarithmicIterator.currentIterationValue);
        }


        public override bool hasNext()
        {
            return integerLogarithmicIterator.hasNext();
        }

        public override DoubleHistogramIterationValue next()
        {
            integerLogarithmicIterator.next();
            return iterationValue;
        }
    }
}
