// Written by Gil Tene of Azul Systems, and released to the public domain,
// as explained at http://creativecommons.org/publicdomain/zero/1.0/
// 
// Ported to .NET by Iulian Margarintescu under the same license and terms as the java version
// Java Version repo: https://github.com/HdrHistogram/HdrHistogram
// Latest ported version is available in the Java submodule in the root of the repo
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using HdrHistogram.ConcurrencyUtilities;

namespace HdrHistogram
{
#pragma warning disable 0659 // GetHashCode does not make sense for a histogram, even if Equals is implemented

    /// <summary>
    /// <h3>An abstract base class for integer values High Dynamic Range (HDR) Histograms</h3>
    /// 
    /// AbstractHistogram supports the recording and analyzing sampled data value counts across a configurable integer value
    /// range with configurable value precision within the range. Value precision is expressed as the number of significant
    /// digits in the value recording, and provides control over value quantization behavior across the value range and the
    /// subsequent value resolution at any given level.
    /// 
    /// For example, a Histogram could be configured to track the counts of observed integer values between 0 and
    /// 3,600,000,000 while maintaining a value precision of 3 significant digits across that range. Value quantization
    /// within the range will thus be no larger than 1/1,000th (or 0.1%) of any value. This example Histogram could
    /// be used to track and analyze the counts of observed response times ranging between 1 microsecond and 1 hour
    /// in magnitude, while maintaining a value resolution of 1 microsecond up to 1 millisecond, a resolution of
    /// 1 millisecond (or better) up to one second, and a resolution of 1 second (or better) up to 1,000 seconds. At it's
    /// maximum tracked value (1 hour), it would still maintain a resolution of 3.6 seconds (or better).
    /// 
    /// See package description for {@link org.HdrHistogram} for details.
    /// </summary>
    public abstract class AbstractHistogram : AbstractHistogramBase, IEquatable<AbstractHistogram>
    {
        // "Hot" accessed fields (used in the the value recording code path) are bunched here, such
        // that they will have a good chance of ending up in the same cache line as the totalCounts and
        // counts array reference fields that subclass implementations will typically add.
        private int leadingZeroCountBase;
        internal protected int subBucketHalfCountMagnitude;
        internal int unitMagnitude;
        internal protected int subBucketHalfCount;
        private long subBucketMask;
        private AtomicLong maxValue = new AtomicLong(0);
        private AtomicLong minNonZeroValue = new AtomicLong(long.MaxValue);

        // Sub-classes will typically add a totalCount field and a counts array field, which will likely be laid out
        // right around here due to the subclass layout rules in most practical JVM implementations.

        //
        //
        //
        // Abstract, counts-type dependent methods to be provided by subclass implementations:
        //
        //
        //

        internal abstract long getCountAtIndex(int index);

        protected abstract long getCountAtNormalizedIndex(int index);

        protected abstract void incrementCountAtIndex(int index);

        protected abstract void addToCountAtIndex(int index, long value);

        protected abstract void setCountAtIndex(int index, long value);

        protected abstract void setCountAtNormalizedIndex(int index, long value);

        protected abstract int getNormalizingIndexOffset();

        protected abstract void setNormalizingIndexOffset(int normalizingIndexOffset);

        protected abstract void shiftNormalizingIndexByOffset(int offsetToAdd, bool lowestHalfBucketPopulated);

        protected abstract void setTotalCount(long totalCount);

        protected abstract void incrementTotalCount();

        protected abstract void addToTotalCount(long value);

        internal protected abstract void clearCounts();

        internal protected abstract int _getEstimatedFootprintInBytes();

        protected internal abstract void resize(long newHighestTrackableValue);

        /**
         * Get the total count of all recorded values in the histogram
         * @return the total count of all recorded values in the histogram
         */
        public abstract long getTotalCount();


        /**
         * Set internally tracked maxValue to new value if new value is greater than current one.
         * May be overridden by subclasses for synchronization or atomicity purposes.
         * @param value new maxValue to set
         */
        protected virtual void updatedMaxValue(long value)
        {
            long current;
            while (value > (current = maxValue.GetValue()))
            {
                maxValue.CompareAndSwap(current, value);
            }
        }

        private void resetMaxValue(long maxValue)
        {
            this.maxValue.SetValue(maxValue);
        }

        /**
         * Set internally tracked minNonZeroValue to new value if new value is smaller than current one.
         * May be overridden by subclasses for synchronization or atomicity purposes.
         * @param value new minNonZeroValue to set
         */
        protected virtual void updateMinNonZeroValue(long value)
        {
            long current;
            while (value < (current = this.minNonZeroValue.GetValue()))
            {
                this.minNonZeroValue.CompareAndSwap(current, value);
            }
        }

        private void resetMinNonZeroValue(long minNonZeroValue)
        {
            this.minNonZeroValue.SetValue(minNonZeroValue);
        }

        //
        //
        //
        // Construction:
        //
        //
        //

        /**
         * Construct an auto-resizing histogram with a lowest discernible value of 1 and an auto-adjusting
         * highestTrackableValue. Can auto-resize up to track values up to (Long.MAX_VALUE / 2).
         *
         * @param numberOfSignificantValueDigits The number of significant decimal digits to which the histogram will
         *                                       maintain value resolution and separation. Must be a non-negative
         *                                       integer between 0 and 5.
         */
        protected AbstractHistogram(int numberOfSignificantValueDigits, int wordSizeInBytes, bool autoResize)
            : this(1, 2, numberOfSignificantValueDigits, wordSizeInBytes, autoResize)
        { }

        /**
         * Construct a histogram given the Lowest and Highest values to be tracked and a number of significant
         * decimal digits. Providing a lowestDiscernibleValue is useful is situations where the units used
         * for the histogram's values are much smaller that the minimal accuracy required. E.g. when tracking
         * time values stated in nanosecond units, where the minimal accuracy required is a microsecond, the
         * proper value for lowestDiscernibleValue would be 1000.
         *
         * @param lowestDiscernibleValue The lowest value that can be discerned (distinguished from 0) by the histogram.
         *                               Must be a positive integer that is {@literal >=} 1. May be internally rounded
         *                               down to nearest power of 2.
         * @param highestTrackableValue The highest value to be tracked by the histogram. Must be a positive
         *                              integer that is {@literal >=} (2 * lowestDiscernibleValue).
         * @param numberOfSignificantValueDigits The number of significant decimal digits to which the histogram will
         *                                       maintain value resolution and separation. Must be a non-negative
         *                                       integer between 0 and 5.
         */
        protected AbstractHistogram(long lowestDiscernibleValue, long highestTrackableValue, int numberOfSignificantValueDigits, int wordSizeInBytes, bool autoResize)
            : base(lowestDiscernibleValue, numberOfSignificantValueDigits, wordSizeInBytes, autoResize)
        {

            if (highestTrackableValue < 2L * lowestDiscernibleValue)
            {
                throw new ArgumentException("highestTrackableValue must be >= 2 * lowestDiscernibleValue");
            }

            init(highestTrackableValue, 1.0, 0);
        }

        /**
         * Construct a histogram with the same range settings as a given source histogram,
         * duplicating the source's start/end timestamps (but NOT it's contents)
         * @param source The source histogram to duplicate
         */
        protected AbstractHistogram(AbstractHistogram source)
            : this(source.getLowestDiscernibleValue(), source.getHighestTrackableValue(), source.getNumberOfSignificantValueDigits(), source.WordSizeInBytes, source.AutoResize)
        {
            this.setStartTimeStamp(source.getStartTimeStamp());
            this.setEndTimeStamp(source.getEndTimeStamp());
        }

        private void init(long highestTrackableValue,
                          double integerToDoubleValueConversionRatio,
                          int normalizingIndexOffset)
        {
            this.highestTrackableValue = highestTrackableValue;
            this.integerToDoubleValueConversionRatio = integerToDoubleValueConversionRatio;
            if (normalizingIndexOffset != 0)
            {
                setNormalizingIndexOffset(normalizingIndexOffset);
            }

            long largestValueWithSingleUnitResolution = 2 * (long)Math.Pow(10, NumberOfSignificantValueDigits);

            unitMagnitude = (int)Math.Floor(Math.Log(LowestDiscernibleValue) / Math.Log(2));

            // We need to maintain power-of-two subBucketCount (for clean direct indexing) that is large enough to
            // provide unit resolution to at least largestValueWithSingleUnitResolution. So figure out
            // largestValueWithSingleUnitResolution's nearest power-of-two (rounded up), and use that:
            int subBucketCountMagnitude = (int)Math.Ceiling(Math.Log(largestValueWithSingleUnitResolution) / Math.Log(2));
            subBucketHalfCountMagnitude = ((subBucketCountMagnitude > 1) ? subBucketCountMagnitude : 1) - 1;
            subBucketCount = (int)Math.Pow(2, (subBucketHalfCountMagnitude + 1));
            subBucketHalfCount = subBucketCount / 2;
            subBucketMask = ((long)subBucketCount - 1) << unitMagnitude;


            // determine exponent range needed to support the trackable value with no overflow:
            establishSize(highestTrackableValue);

            // Establish leadingZeroCountBase, used in getBucketIndex() fast path:
            leadingZeroCountBase = 64 - unitMagnitude - subBucketHalfCountMagnitude - 1;
        }

        protected void establishSize(long newHighestTrackableValue)
        {
            // establish counts array length:
            countsArrayLength = determineArrayLengthNeeded(newHighestTrackableValue);
            // establish exponent range needed to support the trackable value with no overflow:
            bucketCount = getBucketsNeededToCoverValue(newHighestTrackableValue);
            // establish the new highest trackable value:
            highestTrackableValue = newHighestTrackableValue;
        }

        protected int determineArrayLengthNeeded(long highestTrackableValue)
        {
            if (highestTrackableValue < 2L * LowestDiscernibleValue)
            {
                throw new ArgumentException("highestTrackableValue (" + highestTrackableValue + ") cannot be < (2 * lowestDiscernibleValue)");
            }
            //determine counts array length needed:
            int countsArrayLength = getLengthForNumberOfBuckets(getBucketsNeededToCoverValue(highestTrackableValue));
            return countsArrayLength;
        }

        //
        //
        //
        // Value recording support:
        //
        //
        //

        /**
         * Record a value in the histogram
         *
         * @param value The value to be recorded
         * @throws ArrayIndexOutOfBoundsException (may throw) if value is exceeds highestTrackableValue
         */
        public void recordValue(long value)
        {
            recordSingleValue(value);
        }

        /**
         * Record a value in the histogram (adding to the value's current count)
         *
         * @param value The value to be recorded
         * @param count The number of occurrences of this value to record
         * @throws ArrayIndexOutOfBoundsException (may throw) if value is exceeds highestTrackableValue
         */
        public void recordValueWithCount(long value, long count)
        {
            recordCountAtValue(count, value);
        }

        /**
         * Record a value in the histogram.
         * <p>
         * To compensate for the loss of sampled values when a recorded value is larger than the expected
         * interval between value samples, Histogram will auto-generate an additional series of decreasingly-smaller
         * (down to the expectedIntervalBetweenValueSamples) value records.
         * <p>
         * Note: This is a at-recording correction method, as opposed to the post-recording correction method provided
         * by {@link #copyCorrectedForCoordinatedOmission(long)}.
         * The two methods are mutually exclusive, and only one of the two should be be used on a given data set to correct
         * for the same coordinated omission issue.
         * <p>
         * See notes in the description of the Histogram calls for an illustration of why this corrective behavior is
         * important.
         *
         * @param value The value to record
         * @param expectedIntervalBetweenValueSamples If expectedIntervalBetweenValueSamples is larger than 0, add
         *                                           auto-generated value records as appropriate if value is larger
         *                                           than expectedIntervalBetweenValueSamples
         * @throws ArrayIndexOutOfBoundsException (may throw) if value is exceeds highestTrackableValue
         */
        public void recordValueWithExpectedInterval(long value, long expectedIntervalBetweenValueSamples)
        {
            recordSingleValueWithExpectedInterval(value, expectedIntervalBetweenValueSamples);
        }

        /**
         * @deprecated
         *
         * Record a value in the histogram. This deprecated method has identical behavior to
         * <b><code>recordValueWithExpectedInterval()</code></b>. It was renamed to avoid ambiguity.
         *
         * @param value The value to record
         * @param expectedIntervalBetweenValueSamples If expectedIntervalBetweenValueSamples is larger than 0, add
         *                                           auto-generated value records as appropriate if value is larger
         *                                           than expectedIntervalBetweenValueSamples
         * @throws ArrayIndexOutOfBoundsException (may throw) if value is exceeds highestTrackableValue
         */
        public void recordValue(long value, long expectedIntervalBetweenValueSamples)
        {
            recordValueWithExpectedInterval(value, expectedIntervalBetweenValueSamples);
        }

        private void updateMinAndMax(long value)
        {
            if (value > maxValue.GetValue())
            {
                updatedMaxValue(value);
            }
            if ((value < minNonZeroValue.GetValue()) && (value != 0))
            {
                updateMinNonZeroValue(value);
            }
        }

        private void recordCountAtValue(long count, long value)
        {
            int countsIndex = countsArrayIndex(value);
            try
            {
                addToCountAtIndex(countsIndex, count);
            }
            catch (IndexOutOfRangeException ex)
            {
                handleRecordException(count, value, ex);
            }

            updateMinAndMax(value);
            addToTotalCount(count);
        }

        private void recordSingleValue(long value)
        {
            int countsIndex = countsArrayIndex(value);
            try
            {
                incrementCountAtIndex(countsIndex);
            }
            catch (IndexOutOfRangeException ex)
            {
                handleRecordException(1, value, ex);
            }
            updateMinAndMax(value);
            incrementTotalCount();
        }

        private void handleRecordException(long count, long value, Exception ex)
        {
            if (!AutoResize)
            {
                throw new IndexOutOfRangeException("value outside of histogram covered range. Caused by: " + ex);
            }
            resize(value);
            int countsIndex = countsArrayIndex(value);
            addToCountAtIndex(countsIndex, count);
            this.highestTrackableValue = highestEquivalentValue(valueFromIndex(countsArrayLength - 1));
        }

        private void recordValueWithCountAndExpectedInterval(long value, long count, long expectedIntervalBetweenValueSamples)
        {
            recordCountAtValue(count, value);
            if (expectedIntervalBetweenValueSamples <= 0)
                return;
            for (long missingValue = value - expectedIntervalBetweenValueSamples;
                 missingValue >= expectedIntervalBetweenValueSamples;
                 missingValue -= expectedIntervalBetweenValueSamples)
            {
                recordCountAtValue(count, missingValue);
            }
        }

        private void recordSingleValueWithExpectedInterval(long value, long expectedIntervalBetweenValueSamples)
        {
            recordSingleValue(value);
            if (expectedIntervalBetweenValueSamples <= 0)
                return;
            for (long missingValue = value - expectedIntervalBetweenValueSamples;
                missingValue >= expectedIntervalBetweenValueSamples;
                missingValue -= expectedIntervalBetweenValueSamples)
            {
                recordSingleValue(missingValue);
            }
        }

        //
        //
        //
        // Clearing support:
        //
        //
        //

        /**
         * Reset the contents and stats of this histogram
         */
        public void reset()
        {
            clearCounts();
            resetMaxValue(0);
            resetMinNonZeroValue(long.MaxValue);
            setNormalizingIndexOffset(0);
        }

        //
        //
        //
        // Copy support:
        //
        //
        //

        /**
         * Create a copy of this histogram, complete with data and everything.
         *
         * @return A distinct copy of this histogram.
         */
        abstract public AbstractHistogram copy();

        /**
         * Get a copy of this histogram, corrected for coordinated omission.
         * <p>
         * To compensate for the loss of sampled values when a recorded value is larger than the expected
         * interval between value samples, the new histogram will include an auto-generated additional series of
         * decreasingly-smaller (down to the expectedIntervalBetweenValueSamples) value records for each count found
         * in the current histogram that is larger than the expectedIntervalBetweenValueSamples.
         *
         * Note: This is a post-correction method, as opposed to the at-recording correction method provided
         * by {@link #recordValueWithExpectedInterval(long, long) recordValueWithExpectedInterval}. The two
         * methods are mutually exclusive, and only one of the two should be be used on a given data set to correct
         * for the same coordinated omission issue.
         * by
         * <p>
         * See notes in the description of the Histogram calls for an illustration of why this corrective behavior is
         * important.
         *
         * @param expectedIntervalBetweenValueSamples If expectedIntervalBetweenValueSamples is larger than 0, add
         *                                           auto-generated value records as appropriate if value is larger
         *                                           than expectedIntervalBetweenValueSamples
         * @return a copy of this histogram, corrected for coordinated omission.
         */
        abstract public AbstractHistogram copyCorrectedForCoordinatedOmission(long expectedIntervalBetweenValueSamples);

        /**
         * Copy this histogram into the target histogram, overwriting it's contents.
         *
         * @param targetHistogram the histogram to copy into
         */
        public void copyInto(AbstractHistogram targetHistogram)
        {
            targetHistogram.reset();
            targetHistogram.add(this);
            targetHistogram.setStartTimeStamp(this.startTimeStampMsec);
            targetHistogram.setEndTimeStamp(this.endTimeStampMsec);
        }

        /**
         * Copy this histogram, corrected for coordinated omission, into the target histogram, overwriting it's contents.
         * (see {@link #copyCorrectedForCoordinatedOmission} for more detailed explanation about how correction is applied)
         *
         * @param targetHistogram the histogram to copy into
         * @param expectedIntervalBetweenValueSamples If expectedIntervalBetweenValueSamples is larger than 0, add
         *                                           auto-generated value records as appropriate if value is larger
         *                                           than expectedIntervalBetweenValueSamples
         */
        public void copyIntoCorrectedForCoordinatedOmission(AbstractHistogram targetHistogram, long expectedIntervalBetweenValueSamples)
        {
            targetHistogram.reset();
            targetHistogram.addWhileCorrectingForCoordinatedOmission(this, expectedIntervalBetweenValueSamples);
            targetHistogram.setStartTimeStamp(this.startTimeStampMsec);
            targetHistogram.setEndTimeStamp(this.endTimeStampMsec);
        }

        //
        //
        //
        // Add support:
        //
        //
        //

        /**
         * Add the contents of another histogram to this one.
         * <p>
         * As part of adding the contents, the start/end timestamp range of this histogram will be
         * extended to include the start/end timestamp range of the other histogram.
         *
         * @param otherHistogram The other histogram.
         * @throws ArrayIndexOutOfBoundsException (may throw) if values in fromHistogram's are
         * higher than highestTrackableValue.
         */

        public void add(AbstractHistogram otherHistogram)
        {
            long highestRecordableValue = highestEquivalentValue(valueFromIndex(countsArrayLength - 1));
            if (highestRecordableValue < otherHistogram.getMaxValue())
            {
                if (!AutoResize)
                {
                    throw new IndexOutOfRangeException("The other histogram includes values that do not fit in this histogram's range.");
                }
                resize(otherHistogram.getMaxValue());
            }
            if ((bucketCount == otherHistogram.bucketCount) &&
                (subBucketCount == otherHistogram.subBucketCount) &&
                (unitMagnitude == otherHistogram.unitMagnitude) &&
                (getNormalizingIndexOffset() == otherHistogram.getNormalizingIndexOffset()))
            {
                // Counts arrays are of the same length and meaning, so we can just iterate and add directly:
                long observedOtherTotalCount = 0;
                for (int i = 0; i < otherHistogram.countsArrayLength; i++)
                {
                    long otherCount = otherHistogram.getCountAtIndex(i);
                    if (otherCount > 0)
                    {
                        addToCountAtIndex(i, otherCount);
                        observedOtherTotalCount += otherCount;
                    }
                }
                setTotalCount(getTotalCount() + observedOtherTotalCount);
                updatedMaxValue(Math.Max(getMaxValue(), otherHistogram.getMaxValue()));
                updateMinNonZeroValue(Math.Min(getMinNonZeroValue(), otherHistogram.getMinNonZeroValue()));
            }
            else
            {
                // Arrays are not a direct match, so we can't just stream through and add them.
                // Instead, go through the array and add each non-zero value found at it's proper value:
                for (int i = 0; i < otherHistogram.countsArrayLength; i++)
                {
                    long otherCount = otherHistogram.getCountAtIndex(i);
                    if (otherCount > 0)
                    {
                        recordValueWithCount(otherHistogram.valueFromIndex(i), otherCount);
                    }
                }
            }
            setStartTimeStamp(Math.Min(startTimeStampMsec, otherHistogram.startTimeStampMsec));
            setEndTimeStamp(Math.Max(endTimeStampMsec, otherHistogram.endTimeStampMsec));
        }

        /**
         * Subtract the contents of another histogram from this one.
         * <p>
         * The start/end timestamps of this histogram will remain unchanged.
         *
         * @param otherHistogram The other histogram.
         * @throws ArrayIndexOutOfBoundsException (may throw) if values in otherHistogram's are higher than highestTrackableValue.
         *
         */

        public void subtract(AbstractHistogram otherHistogram)
        {
            long highestRecordableValue = valueFromIndex(countsArrayLength - 1);
            if (highestRecordableValue < otherHistogram.getMaxValue())
            {
                if (!AutoResize)
                {
                    throw new IndexOutOfRangeException("The other histogram includes values that do not fit in this histogram's range.");
                }
                resize(otherHistogram.getMaxValue());
            }
            if ((bucketCount == otherHistogram.bucketCount) &&
                (subBucketCount == otherHistogram.subBucketCount) &&
                (unitMagnitude == otherHistogram.unitMagnitude) &&
                (getNormalizingIndexOffset() == otherHistogram.getNormalizingIndexOffset()))
            {
                // Counts arrays are of the same length and meaning, so we can just iterate and add directly:
                long observedOtherTotalCount = 0;
                for (int i = 0; i < otherHistogram.countsArrayLength; i++)
                {
                    long otherCount = otherHistogram.getCountAtIndex(i);
                    if (otherCount > 0)
                    {
                        if (getCountAtIndex(i) < otherCount)
                        {
                            throw new ArgumentException("otherHistogram count (" + otherCount + ") at value " +
                                                        valueFromIndex(i) + " is larger than this one's (" + getCountAtIndex(i) + ")");
                        }
                        addToCountAtIndex(i, -otherCount);
                        observedOtherTotalCount += otherCount;
                    }
                }
                setTotalCount(getTotalCount() - observedOtherTotalCount);
                updatedMaxValue(Math.Max(getMaxValue(), otherHistogram.getMaxValue()));
                updateMinNonZeroValue(Math.Min(getMinNonZeroValue(), otherHistogram.getMinNonZeroValue()));
            }
            else
            {
                // Arrays are not a direct match, so we can't just stream through and add them.
                // Instead, go through the array and add each non-zero value found at it's proper value:
                for (int i = 0; i < otherHistogram.countsArrayLength; i++)
                {
                    long otherCount = otherHistogram.getCountAtIndex(i);
                    if (otherCount > 0)
                    {
                        long otherValue = otherHistogram.valueFromIndex(i);
                        if (getCountAtValue(otherValue) < otherCount)
                        {
                            throw new ArgumentException("otherHistogram count (" + otherCount + ") at value " +
                                                        otherValue + " is larger than this one's (" + getCountAtValue(otherValue) + ")");
                        }
                        recordValueWithCount(otherValue, -otherCount);
                    }
                }
            }
            // With subtraction, the max and minNonZero values could have changed:
            if ((getCountAtValue(getMaxValue()) <= 0) || getCountAtValue(getMinNonZeroValue()) <= 0)
            {
                establishInternalTackingValues();
            }
        }

        /**
         * Add the contents of another histogram to this one, while correcting the incoming data for coordinated omission.
         * <p>
         * To compensate for the loss of sampled values when a recorded value is larger than the expected
         * interval between value samples, the values added will include an auto-generated additional series of
         * decreasingly-smaller (down to the expectedIntervalBetweenValueSamples) value records for each count found
         * in the current histogram that is larger than the expectedIntervalBetweenValueSamples.
         *
         * Note: This is a post-recording correction method, as opposed to the at-recording correction method provided
         * by {@link #recordValueWithExpectedInterval(long, long) recordValueWithExpectedInterval}. The two
         * methods are mutually exclusive, and only one of the two should be be used on a given data set to correct
         * for the same coordinated omission issue.
         * by
         * <p>
         * See notes in the description of the Histogram calls for an illustration of why this corrective behavior is
         * important.
         *
         * @param otherHistogram The other histogram. highestTrackableValue and largestValueWithSingleUnitResolution must match.
         * @param expectedIntervalBetweenValueSamples If expectedIntervalBetweenValueSamples is larger than 0, add
         *                                           auto-generated value records as appropriate if value is larger
         *                                           than expectedIntervalBetweenValueSamples
         * @throws ArrayIndexOutOfBoundsException (may throw) if values exceed highestTrackableValue
         */
        public void addWhileCorrectingForCoordinatedOmission(AbstractHistogram otherHistogram, long expectedIntervalBetweenValueSamples)
        {
            AbstractHistogram toHistogram = this;

            foreach (HistogramIterationValue v in otherHistogram.RecordedValues())
            {
                toHistogram.recordValueWithCountAndExpectedInterval(v.getValueIteratedTo(),
                        v.getCountAtValueIteratedTo(), expectedIntervalBetweenValueSamples);
            }
        }

        //
        //
        //
        // Shifting support:
        //
        //
        //

        /**
         * Shift recorded values to the left (the equivalent of a &lt;&lt; shift operation on all recorded values). The
         * configured integer value range limits and value precision setting will remain unchanged.
         *
         * An {@link ArrayIndexOutOfBoundsException} will be thrown if any recorded values may be lost
         * as a result of the attempted operation, reflecting an "overflow" conditions. Expect such an overflow
         * exception if the operation would cause the current maxValue to be scaled to a value that is outside
         * of the covered value range.
         *
         * @param numberOfBinaryOrdersOfMagnitude The number of binary orders of magnitude to shift by
         */
        protected internal virtual void shiftValuesLeft(int numberOfBinaryOrdersOfMagnitude)
        {
            if (numberOfBinaryOrdersOfMagnitude < 0)
            {
                throw new ArgumentException("Cannot shift by a negative number of magnitudes");
            }

            if (numberOfBinaryOrdersOfMagnitude == 0)
            {
                return;
            }
            if (getTotalCount() == getCountAtIndex(0))
            {
                // (no need to shift any values if all recorded values are at the 0 value level:)
                return;
            }

            int shiftAmount = numberOfBinaryOrdersOfMagnitude << subBucketHalfCountMagnitude;
            int maxValueIndex = countsArrayIndex(getMaxValue());
            // indicate overflow if maxValue is in the range being wrapped:
            if (maxValueIndex >= (countsArrayLength - shiftAmount))
            {
                throw new IndexOutOfRangeException("Operation would overflow, would discard recorded value counts");
            }

            long maxValueBeforeShift = this.maxValue.GetAndSet(0);
            long minNonZeroValueBeforeShift = this.minNonZeroValue.GetAndSet(long.MaxValue);

            bool lowestHalfBucketPopulated = (minNonZeroValueBeforeShift < subBucketHalfCount);

            // Perform the shift:
            shiftNormalizingIndexByOffset(shiftAmount, lowestHalfBucketPopulated);

            // adjust min, max:
            updateMinAndMax(maxValueBeforeShift << numberOfBinaryOrdersOfMagnitude);
            if (minNonZeroValueBeforeShift < long.MaxValue)
            {
                updateMinAndMax(minNonZeroValueBeforeShift << numberOfBinaryOrdersOfMagnitude);
            }
        }

        protected void nonConcurrentNormalizingIndexShift(int shiftAmount, bool lowestHalfBucketPopulated)
        {
            // Save and clear the 0 value count:
            long zeroValueCount = getCountAtIndex(0);
            setCountAtIndex(0, 0);

            setNormalizingIndexOffset(getNormalizingIndexOffset() + shiftAmount);

            // Deal with lower half bucket if needed:
            if (lowestHalfBucketPopulated)
            {
                shiftLowestHalfBucketContentsLeft(shiftAmount);
            }

            // Restore the 0 value count:
            setCountAtIndex(0, zeroValueCount);
        }

        void shiftLowestHalfBucketContentsLeft(int shiftAmount)
        {
            int numberOfBinaryOrdersOfMagnitude = shiftAmount >> subBucketHalfCountMagnitude;

            // The lowest half-bucket (not including the 0 value) is special: unlike all other half
            // buckets, the lowest half bucket values cannot be scaled by simply changing the
            // normalizing offset. Instead, they must be individually re-recorded at the new
            // scale, and cleared from the current one.
            //
            // We know that all half buckets "below" the current lowest one are full of 0s, because
            // we would have overflowed otherwise. So we need to shift the values in the current
            // lowest half bucket into that range (including the current lowest half bucket itself).
            // Iterating up from the lowermost non-zero "from slot" and copying values to the newly
            // scaled "to slot" (and then zeroing the "from slot"), will work in a single pass,
            // because the scale "to slot" index will always be a lower index than its or any
            // preceding non-scaled "from slot" index:
            //
            // (Note that we specifically avoid slot 0, as it is directly handled in the outer case)

            for (int fromIndex = 1; fromIndex < subBucketHalfCount; fromIndex++)
            {
                long toValue = valueFromIndex(fromIndex) << numberOfBinaryOrdersOfMagnitude;
                int toIndex = countsArrayIndex(toValue);
                long countAtFromIndex = getCountAtNormalizedIndex(fromIndex);
                setCountAtIndex(toIndex, countAtFromIndex);
                setCountAtNormalizedIndex(fromIndex, 0);
            }

            // Note that the above loop only creates O(N) work for histograms that have values in
            // the lowest half-bucket (excluding the 0 value). Histograms that never have values
            // there (e.g. all integer value histograms used as internal storage in DoubleHistograms)
            // will never loop, and their shifts will remain O(1).
        }

        /**
         * Shift recorded values to the right (the equivalent of a &gt;&gt; shift operation on all recorded values). The
         * configured integer value range limits and value precision setting will remain unchanged.
         * <p>
         * Shift right operations that do not underflow are reversible with a shift left operation with no loss of
         * information. An {@link ArrayIndexOutOfBoundsException} reflecting an "underflow" conditions will be thrown
         * if any recorded values may lose representation accuracy as a result of the attempted shift operation.
         * <p>
         * For a shift of a single order of magnitude, expect such an underflow exception if any recorded non-zero
         * values up to [numberOfSignificantValueDigits (rounded up to nearest power of 2) multiplied by
         * (2 ^ numberOfBinaryOrdersOfMagnitude) currently exist in the histogram.
         *
         * @param numberOfBinaryOrdersOfMagnitude The number of binary orders of magnitude to shift by
         */

        protected internal virtual void shiftValuesRight(int numberOfBinaryOrdersOfMagnitude)
        {
            if (numberOfBinaryOrdersOfMagnitude < 0)
            {
                throw new ArgumentException("Cannot shift by a negative number of magnitudes");
            }

            if (numberOfBinaryOrdersOfMagnitude == 0)
            {
                return;
            }
            if (getTotalCount() == getCountAtIndex(0))
            {
                // (no need to shift any values if all recorded values are at the 0 value level:)
                return;
            }

            int shiftAmount = subBucketHalfCount * numberOfBinaryOrdersOfMagnitude;

            // indicate underflow if minValue is in the range being shifted from:
            int minNonZeroValueIndex = countsArrayIndex(getMinNonZeroValue());
            // Any shifting into the bottom-most half bucket would represents a loss of accuracy,
            // and a non-reversible operation. Therefore any non-0 value that falls in an
            // index below (shiftAmount + subBucketHalfCount) would represent an underflow:
            if (minNonZeroValueIndex < shiftAmount + subBucketHalfCount)
            {
                throw new IndexOutOfRangeException("Operation would underflow and lose precision of already recorded value counts");
            }

            // perform shift:

            long maxValueBeforeShift = this.maxValue.GetAndSet(0);
            long minNonZeroValueBeforeShift = this.minNonZeroValue.GetAndSet(long.MaxValue);

            // move normalizingIndexOffset
            shiftNormalizingIndexByOffset(-shiftAmount, false);

            // adjust min, max:
            updateMinAndMax(maxValueBeforeShift >> numberOfBinaryOrdersOfMagnitude);
            if (minNonZeroValueBeforeShift < long.MaxValue)
            {
                updateMinAndMax(minNonZeroValueBeforeShift >> numberOfBinaryOrdersOfMagnitude);
            }
        }

        //
        //
        //
        // Comparison support:
        //
        //
        //

        /// <summary>
        /// Determine if this histogram is equivalent to another.
        /// </summary>
        /// <param name="other">the other histogram to compare to</param>
        /// <returns>True if this histogram are equivalent with the other.</returns>
        public bool Equals(AbstractHistogram other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other == null)
            {
                return false;
            }

            if ((LowestDiscernibleValue != other.LowestDiscernibleValue) ||
                (highestTrackableValue != other.highestTrackableValue) ||
                (NumberOfSignificantValueDigits != other.NumberOfSignificantValueDigits) ||
                (integerToDoubleValueConversionRatio != other.integerToDoubleValueConversionRatio))
            {
                return false;
            }
            if (countsArrayLength != other.countsArrayLength)
            {
                return false;
            }
            if (getTotalCount() != other.getTotalCount())
            {
                return false;
            }
            for (int i = 0; i < countsArrayLength; i++)
            {
                if (getCountAtIndex(i) != other.getCountAtIndex(i))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Determine if this histogram is equivalent to another.
        /// </summary>
        /// <param name="other">the other histogram to compare to</param>
        /// <returns>True if this histogram are equivalent with the other.</returns>
        public override bool Equals(object other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Equals(other as AbstractHistogram);
        }

        //
        //
        //
        // Histogram structure querying support:
        //
        //
        //

        /**
         * get the configured lowestDiscernibleValue
         * @return lowestDiscernibleValue
         */
        public long getLowestDiscernibleValue()
        {
            return LowestDiscernibleValue;
        }

        /**
         * get the configured highestTrackableValue
         * @return highestTrackableValue
         */
        public long getHighestTrackableValue()
        {
            return highestTrackableValue;
        }

        /**
         * get the configured numberOfSignificantValueDigits
         * @return numberOfSignificantValueDigits
         */
        public int getNumberOfSignificantValueDigits()
        {
            return NumberOfSignificantValueDigits;
        }

        /**
         * Get the size (in value units) of the range of values that are equivalent to the given value within the
         * histogram's resolution. Where "equivalent" means that value samples recorded for any two
         * equivalent values are counted in a common total count.
         *
         * @param value The given value
         * @return The lowest value that is equivalent to the given value within the histogram's resolution.
         */
        public long sizeOfEquivalentValueRange(long value)
        {
            int bucketIndex = getBucketIndex(value);
            int subBucketIndex = getSubBucketIndex(value, bucketIndex);
            long distanceToNextValue =
                    (1L << (unitMagnitude + ((subBucketIndex >= subBucketCount) ? (bucketIndex + 1) : bucketIndex)));
            return distanceToNextValue;
        }

        /**
         * Get the lowest value that is equivalent to the given value within the histogram's resolution.
         * Where "equivalent" means that value samples recorded for any two
         * equivalent values are counted in a common total count.
         *
         * @param value The given value
         * @return The lowest value that is equivalent to the given value within the histogram's resolution.
         */
        public long lowestEquivalentValue(long value)
        {
            int bucketIndex = getBucketIndex(value);
            int subBucketIndex = getSubBucketIndex(value, bucketIndex);
            long thisValueBaseLevel = valueFromIndex(bucketIndex, subBucketIndex);
            return thisValueBaseLevel;
        }

        /**
         * Get the highest value that is equivalent to the given value within the histogram's resolution.
         * Where "equivalent" means that value samples recorded for any two
         * equivalent values are counted in a common total count.
         *
         * @param value The given value
         * @return The highest value that is equivalent to the given value within the histogram's resolution.
         */
        public long highestEquivalentValue(long value)
        {
            return nextNonEquivalentValue(value) - 1;
        }

        /**
         * Get a value that lies in the middle (rounded up) of the range of values equivalent the given value.
         * Where "equivalent" means that value samples recorded for any two
         * equivalent values are counted in a common total count.
         *
         * @param value The given value
         * @return The value lies in the middle (rounded up) of the range of values equivalent the given value.
         */
        public long medianEquivalentValue(long value)
        {
            return (lowestEquivalentValue(value) + (sizeOfEquivalentValueRange(value) >> 1));
        }

        /**
         * Get the next value that is not equivalent to the given value within the histogram's resolution.
         * Where "equivalent" means that value samples recorded for any two
         * equivalent values are counted in a common total count.
         *
         * @param value The given value
         * @return The next value that is not equivalent to the given value within the histogram's resolution.
         */
        public long nextNonEquivalentValue(long value)
        {
            return lowestEquivalentValue(value) + sizeOfEquivalentValueRange(value);
        }

        /**
         * Determine if two values are equivalent with the histogram's resolution.
         * Where "equivalent" means that value samples recorded for any two
         * equivalent values are counted in a common total count.
         *
         * @param value1 first value to compare
         * @param value2 second value to compare
         * @return True if values are equivalent with the histogram's resolution.
         */
        public bool valuesAreEquivalent(long value1, long value2)
        {
            return (lowestEquivalentValue(value1) == lowestEquivalentValue(value2));
        }

        /**
         * Provide a (conservatively high) estimate of the Histogram's total footprint in bytes
         *
         * @return a (conservatively high) estimate of the Histogram's total footprint in bytes
         */
        public int getEstimatedFootprintInBytes()
        {
            return _getEstimatedFootprintInBytes();
        }

        //
        //
        //
        // Timestamp support:
        //
        //
        //

        /**
         * get the start time stamp [optionally] stored with this histogram
         * @return the start time stamp [optionally] stored with this histogram
         */
        public override long getStartTimeStamp()
        {
            return startTimeStampMsec;
        }

        /**
         * Set the start time stamp value associated with this histogram to a given value.
         * @param timeStampMsec the value to set the time stamp to, [by convention] in msec since the epoch.
         */
        public override void setStartTimeStamp(long timeStampMsec)
        {
            this.startTimeStampMsec = timeStampMsec;
        }

        /**
         * get the end time stamp [optionally] stored with this histogram
         * @return the end time stamp [optionally] stored with this histogram
         */
        public override long getEndTimeStamp()
        {
            return endTimeStampMsec;
        }

        /**
         * Set the end time stamp value associated with this histogram to a given value.
         * @param timeStampMsec the value to set the time stamp to, [by convention] in msec since the epoch.
         */
        public override void setEndTimeStamp(long timeStampMsec)
        {
            this.endTimeStampMsec = timeStampMsec;
        }

        //
        //
        //
        // Histogram Data access support:
        //
        //
        //

        /**
         * Get the lowest recorded value level in the histogram
         *
         * @return the Min value recorded in the histogram
         */

        public long getMinValue()
        {
            if ((getCountAtIndex(0) > 0) || (getTotalCount() == 0))
            {
                return 0;
            }
            return getMinNonZeroValue();
        }

        /**
         * Get the highest recorded value level in the histogram
         *
         * @return the Max value recorded in the histogram
         */

        public long getMaxValue()
        {
            var maxValue = this.maxValue.GetValue();
            return (maxValue == 0) ? 0 : highestEquivalentValue(maxValue);
        }

        /**
         * Get the lowest recorded non-zero value level in the histogram
         *
         * @return the lowest recorded non-zero value level in the histogram
         */
        public long getMinNonZeroValue()
        {
            var minNonZeroValue = this.minNonZeroValue.GetValue();
            return (minNonZeroValue == long.MaxValue) ? long.MaxValue : lowestEquivalentValue(minNonZeroValue);
        }

        /**
         * Get the highest recorded value level in the histogram as a double
         *
         * @return the Max value recorded in the histogram
         */
        public override double getMaxValueAsDouble()
        {
            return getMaxValue();
        }

        /**
         * Get the computed mean value of all recorded values in the histogram
         *
         * @return the mean value (in value units) of the histogram data
         */

        public double getMean()
        {
            if (getTotalCount() == 0)
            {
                return 0.0;
            }
            recordedValuesIterator.reset();
            double totalValue = 0;
            while (recordedValuesIterator.hasNext())
            {
                HistogramIterationValue iterationValue = recordedValuesIterator.next();
                totalValue += medianEquivalentValue(iterationValue.getValueIteratedTo())
                              * iterationValue.getCountAtValueIteratedTo();
            }
            return (totalValue * 1.0) / getTotalCount();
        }

        /**
         * Get the computed standard deviation of all recorded values in the histogram
         *
         * @return the standard deviation (in value units) of the histogram data
         */

        public double getStdDeviation()
        {
            if (getTotalCount() == 0)
            {
                return 0.0;
            }
            double mean = getMean();
            double geometric_deviation_total = 0.0;
            recordedValuesIterator.reset();
            while (recordedValuesIterator.hasNext())
            {
                HistogramIterationValue iterationValue = recordedValuesIterator.next();
                Double deviation = (medianEquivalentValue(iterationValue.getValueIteratedTo()) * 1.0) - mean;
                geometric_deviation_total += (deviation * deviation) * iterationValue.getCountAddedInThisIterationStep();
            }
            double std_deviation = Math.Sqrt(geometric_deviation_total / getTotalCount());
            return std_deviation;
        }

        /**
         * Get the value at a given percentile.
         * When the given percentile is &gt; 0.0, the value returned is the value that the given
         * percentage of the overall recorded value entries in the histogram are either smaller than
         * or equivalent to. When the given percentile is 0.0, the value returned is the value that all value
         * entries in the histogram are either larger than or equivalent to.
         * <p>
         * Note that two values are "equivalent" in this statement if
         * {@link org.HdrHistogram.AbstractHistogram#valuesAreEquivalent} would return true.
         *
         * @param percentile  The percentile for which to return the associated value
         * @return The value that the given percentage of the overall recorded value entries in the
         * histogram are either smaller than or equivalent to. When the percentile is 0.0, returns the
         * value that all value entries in the histogram are either larger than or equivalent to.
         */

        public long getValueAtPercentile(double percentile)
        {
            double requestedPercentile = Math.Min(percentile, 100.0); // Truncate down to 100%
            long countAtPercentile = (long)(((requestedPercentile / 100.0) * getTotalCount()) + 0.5); // round to nearest
            countAtPercentile = Math.Max(countAtPercentile, 1); // Make sure we at least reach the first recorded entry
            long totalToCurrentIndex = 0;
            for (int i = 0; i < countsArrayLength; i++)
            {
                totalToCurrentIndex += getCountAtIndex(i);
                if (totalToCurrentIndex >= countAtPercentile)
                {
                    long valueAtIndex = valueFromIndex(i);
                    return (percentile == 0.0) ?
                        lowestEquivalentValue(valueAtIndex) :
                        highestEquivalentValue(valueAtIndex);
                }
            }
            return 0;
        }

        /**
         * Get the percentile at a given value.
         * The percentile returned is the percentile of values recorded in the histogram that are smaller
         * than or equivalent to the given value.
         * <p>
         * Note that two values are "equivalent" in this statement if
         * {@link org.HdrHistogram.AbstractHistogram#valuesAreEquivalent} would return true.
         *
         * @param value The value for which to return the associated percentile
         * @return The percentile of values recorded in the histogram that are smaller than or equivalent
         * to the given value.
         */

        public double getPercentileAtOrBelowValue(long value)
        {
            if (getTotalCount() == 0)
            {
                return 100.0;
            }
            int targetIndex = Math.Min(countsArrayIndex(value), (countsArrayLength - 1));
            long totalToCurrentIndex = 0;
            for (int i = 0; i <= targetIndex; i++)
            {
                totalToCurrentIndex += getCountAtIndex(i);
            }
            return (100.0 * totalToCurrentIndex) / getTotalCount();
        }

        /**
         * Get the count of recorded values within a range of value levels (inclusive to within the histogram's resolution).
         *
         * @param lowValue  The lower value bound on the range for which
         *                  to provide the recorded count. Will be rounded down with
         *                  {@link Histogram#lowestEquivalentValue lowestEquivalentValue}.
         * @param highValue  The higher value bound on the range for which to provide the recorded count.
         *                   Will be rounded up with {@link Histogram#highestEquivalentValue highestEquivalentValue}.
         * @return the total count of values recorded in the histogram within the value range that is
         * {@literal >=} lowestEquivalentValue(<i>lowValue</i>) and {@literal <=} highestEquivalentValue(<i>highValue</i>)
         */

        public long getCountBetweenValues(long lowValue, long highValue)
        {
            int lowIndex = Math.Max(0, countsArrayIndex(lowValue));
            int highIndex = Math.Min(countsArrayIndex(highValue), (countsArrayLength - 1));
            long count = 0;
            for (int i = lowIndex; i <= highIndex; i++)
            {
                count += getCountAtIndex(i);
            }
            return count;
        }

        /**
         * Get the count of recorded values at a specific value (to within the histogram resolution at the value level).
         *
         * @param value The value for which to provide the recorded count
         * @return The total count of values recorded in the histogram within the value range that is
         * {@literal >=} lowestEquivalentValue(<i>value</i>) and {@literal <=} highestEquivalentValue(<i>value</i>)
         */
        public long getCountAtValue(long value)
        {
            int index = Math.Min(Math.Max(0, countsArrayIndex(value)), (countsArrayLength - 1));
            return getCountAtIndex(index);
        }

        //
        //
        //
        // Serialization support:
        //
        //
        //

        internal protected void writeObject(BinaryWriter o)
        {
            o.Write(LowestDiscernibleValue);
            o.Write(highestTrackableValue);
            o.Write(NumberOfSignificantValueDigits);
            o.Write(getNormalizingIndexOffset());
            o.Write(integerToDoubleValueConversionRatio);
            o.Write(getTotalCount());
            // Max Value is added to the serialized form because establishing max via scanning is "harder" during
            // deserialization, as the counts array is not available at the subclass deserializing level, and we don't
            // really want to have each subclass establish max on it's own...
            o.Write(maxValue.GetValue());
            o.Write(minNonZeroValue.GetValue());
            o.Write(startTimeStampMsec);
            o.Write(endTimeStampMsec);
            o.Write(AutoResize);
        }

        private void readObject(BinaryReader o)
        {
            long lowestDiscernibleValue = o.ReadInt64();
            long highestTrackableValue = o.ReadInt64();
            int numberOfSignificantValueDigits = o.ReadInt32();
            int normalizingIndexOffset = o.ReadInt32();
            double integerToDoubleValueConversionRatio = o.ReadDouble();
            long indicatedTotalCount = o.ReadInt64();
            long indicatedMaxValue = o.ReadInt64();
            long indicatedMinNonZeroValue = o.ReadInt64();
            long indicatedStartTimeStampMsec = o.ReadInt64();
            long indicatedEndTimeStampMsec = o.ReadInt64();
            bool indicatedAutoResize = o.ReadBoolean();

            // TODO: serialization - numberOfSignificantValueDigits is readonly
            //init(lowestDiscernibleValue, highestTrackableValue, numberOfSignificantValueDigits,
            //        integerToDoubleValueConversionRatio, normalizingIndexOffset);
            // Set internalTrackingValues (can't establish them from array yet, because it's not yet read...)
            setTotalCount(indicatedTotalCount);
            maxValue.SetValue(indicatedMaxValue);
            minNonZeroValue.SetValue(indicatedMinNonZeroValue);
            startTimeStampMsec = indicatedStartTimeStampMsec;
            endTimeStampMsec = indicatedEndTimeStampMsec;

            // TODO: serialization - autoResize is readonly
            //autoResize = indicatedAutoResize;
        }

        //
        //
        //
        // Encoding/Decoding support:
        //
        //
        //

        /**
         * Get the capacity needed to encode this histogram into a ByteBuffer
         * @return the capacity needed to encode this histogram into a ByteBuffer
         */
        //@Override
        public override int getNeededByteBufferCapacity()
        {
            return getNeededByteBufferCapacity(countsArrayLength);
        }

        private static int ENCODING_HEADER_SIZE = 40;
        private static int V0_ENCODING_HEADER_SIZE = 32;

        internal int getNeededByteBufferCapacity(int relevantLength)
        {
            return getNeededPayloadByteBufferCapacity(relevantLength) + ENCODING_HEADER_SIZE;
        }

        int getNeededPayloadByteBufferCapacity(int relevantLength)
        {
            return (relevantLength * WordSizeInBytes);
        }

        internal protected abstract void fillCountsArrayFromBuffer(ByteBuffer buffer, int length);

        internal protected abstract void fillBufferFromCountsArray(ByteBuffer buffer, int length);

        private static int V0EncodingCookieBase = 0x1c849308;
        private static int V0EcompressedEncodingCookieBase = 0x1c849309;

        private static int encodingCookieBase = 0x1c849301;
        private static int compressedEncodingCookieBase = 0x1c849302;

        private int getV0EncodingCookie()
        {
            return V0EncodingCookieBase + (WordSizeInBytes << 4);
        }

        private int getEncodingCookie()
        {
            return encodingCookieBase + (WordSizeInBytes << 4);
        }

        private int getCompressedEncodingCookie()
        {
            return compressedEncodingCookieBase + (WordSizeInBytes << 4);
        }

        private static int getCookieBase(int cookie)
        {
            return (cookie & ~0xf0);
        }

        private static int getWordSizeInBytesFromCookie(int cookie)
        {
            return (cookie & 0xf0) >> 4;
        }

        /**
         * Encode this histogram into a ByteBuffer
         * @param buffer The buffer to encode into
         * @return The number of bytes written to the buffer
         */
        [MethodImpl(MethodImplOptions.Synchronized)]
        public int encodeIntoByteBuffer(ByteBuffer buffer)
        {
            long maxValue = getMaxValue();
            int relevantLength = countsArrayIndex(maxValue) + 1;
            if (buffer.capacity() < getNeededByteBufferCapacity(relevantLength))
            {
                throw new IndexOutOfRangeException("buffer does not have capacity for" +
                        getNeededByteBufferCapacity(relevantLength) + " bytes");
            }
            int initialPosition = buffer.position();
            buffer.putInt(getEncodingCookie());
            buffer.putInt(relevantLength * WordSizeInBytes);
            buffer.putInt(getNormalizingIndexOffset());
            buffer.putInt(NumberOfSignificantValueDigits);
            buffer.putLong(LowestDiscernibleValue);
            buffer.putLong(highestTrackableValue);
            buffer.putDouble(integerToDoubleValueConversionRatio);

            fillBufferFromCountsArray(buffer, relevantLength);

            int bytesWritten = getNeededByteBufferCapacity(relevantLength);
            buffer.position(initialPosition + bytesWritten);
            return bytesWritten;
        }

        /**
         * Encode this histogram in compressed form into a byte array
         * @param targetBuffer The buffer to encode into
         * @param compressionLevel Compression level (for java.util.zip.Deflater).
         * @return The number of bytes written to the buffer
         */
        //@Override
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override int encodeIntoCompressedByteBuffer(ByteBuffer targetBuffer, int compressionLevel)
        {
            int neededCapacity = getNeededByteBufferCapacity(countsArrayLength);
            if (intermediateUncompressedByteBuffer == null || intermediateUncompressedByteBuffer.capacity() < neededCapacity)
            {
                intermediateUncompressedByteBuffer = ByteBuffer.allocate(neededCapacity);
            }
            intermediateUncompressedByteBuffer.clear();
            int uncompressedLength = encodeIntoByteBuffer(intermediateUncompressedByteBuffer);

            int initialTargetPosition = targetBuffer.position();
            targetBuffer.putInt(getCompressedEncodingCookie());
            targetBuffer.putInt(0); // Placeholder for compressed contents length

            Deflater compressor = new Deflater(compressionLevel);
            compressor.setInput(intermediateUncompressedByteBuffer.array(), 0, uncompressedLength);
            compressor.finish();

            byte[] targetArray = targetBuffer.array();
            int compressedTargetOffset = initialTargetPosition + 8;
            int compressedDataLength =
                    compressor.deflate(
                            targetArray,
                            compressedTargetOffset,
                            targetArray.Length - compressedTargetOffset
                    );
            compressor.end();

            targetBuffer.putInt(initialTargetPosition + 4, compressedDataLength); // Record the compressed length
            int bytesWritten = compressedDataLength + 8;
            targetBuffer.position(initialTargetPosition + bytesWritten);
            return bytesWritten;
        }

        /**
         * Encode this histogram in compressed form into a byte array
         * @param targetBuffer The buffer to encode into
         * @return The number of bytes written to the array
         */
        //public int encodeIntoCompressedByteBuffer(ByteBuffer targetBuffer)
        //{
        //    return encodeIntoCompressedByteBuffer(targetBuffer, Deflater.DEFAULT_COMPRESSION);
        //}

        private static Type[] constructorArgsTypes = { typeof(long), typeof(long), typeof(int) };

        internal static AbstractHistogram decodeFromByteBuffer(ByteBuffer buffer, Type histogramClass, long minBarForHighestTrackableValue)
        {
            return decodeFromByteBuffer(buffer, histogramClass, minBarForHighestTrackableValue, null, null);
        }

        internal static AbstractHistogram decodeFromByteBuffer(
                ByteBuffer buffer,
                Type histogramClass,
                long minBarForHighestTrackableValue,
                Inflater decompressor,
                ByteBuffer intermediateUncompressedByteBuffer)
        {

            int cookie = buffer.getInt();
            int payloadLength;
            int normalizingIndexOffset;
            int numberOfSignificantValueDigits;
            long lowestTrackableUnitValue;
            long highestTrackableValue;
            double integerToDoubleValueConversionRatio;

            if (getCookieBase(cookie) == encodingCookieBase)
            {
                payloadLength = buffer.getInt();
                normalizingIndexOffset = buffer.getInt();
                numberOfSignificantValueDigits = buffer.getInt();
                lowestTrackableUnitValue = buffer.getLong();
                highestTrackableValue = buffer.getLong();
                integerToDoubleValueConversionRatio = buffer.getDouble();
            }
            else if (getCookieBase(cookie) == V0EncodingCookieBase)
            {
                numberOfSignificantValueDigits = buffer.getInt();
                lowestTrackableUnitValue = buffer.getLong();
                highestTrackableValue = buffer.getLong();
                buffer.getLong(); // Discard totalCount field in V0 header.
                payloadLength = int.MaxValue;
                integerToDoubleValueConversionRatio = 1.0;
                normalizingIndexOffset = 0;
            }
            else
            {
                throw new ArgumentException("The buffer does not contain a Histogram");
            }
            highestTrackableValue = Math.Max(highestTrackableValue, minBarForHighestTrackableValue);

            ;

            // Construct histogram:
            //@SuppressWarnings("unchecked")
            var constructor = histogramClass.GetConstructor(constructorArgsTypes);
            AbstractHistogram histogram = constructor.Invoke(new object[] { lowestTrackableUnitValue, highestTrackableValue, numberOfSignificantValueDigits })
                as AbstractHistogram;
            //    Constructor<AbstractHistogram> constructor = histogramClass.getConstructor(constructorArgsTypes);
            //histogram = constructor.newInstance(lowestTrackableUnitValue, highestTrackableValue,
            //        numberOfSignificantValueDigits);

            histogram.integerToDoubleValueConversionRatio = integerToDoubleValueConversionRatio;
            histogram.setNormalizingIndexOffset(normalizingIndexOffset);
            if ((cookie != histogram.getEncodingCookie()) &&
                    (cookie != histogram.getV0EncodingCookie()))
            {
                throw new ArgumentException(
                        "The buffer's encoded value byte size (" +
                                getWordSizeInBytesFromCookie(cookie) +
                                ") does not match the Histogram's (" +
                                histogram.WordSizeInBytes + ")");
            }


            ByteBuffer payLoadSourceBuffer;

            int expectedCapacity =
                    Math.Min(
                            histogram.getNeededPayloadByteBufferCapacity(histogram.countsArrayLength),
                            payloadLength
                    );

            if (decompressor == null)
            {
                // No compressed source buffer. Payload is in buffer, after header.
                if (expectedCapacity > buffer.remaining())
                {
                    throw new ArgumentException("The buffer does not contain the full Histogram payload");
                }
                payLoadSourceBuffer = buffer;
            }
            else
            {
                // Compressed source buffer. Payload needs to be decoded from there.
                payLoadSourceBuffer = intermediateUncompressedByteBuffer;
                if (payLoadSourceBuffer == null)
                {
                    payLoadSourceBuffer = ByteBuffer.allocate(expectedCapacity);
                }
                else
                {
                    payLoadSourceBuffer.reset();
                    if (payLoadSourceBuffer.remaining() < expectedCapacity)
                    {
                        throw new ArgumentException("Supplied intermediate not large enough (capacity = " +
                                payLoadSourceBuffer.capacity() + ", expected = " + expectedCapacity);
                    }
                    payLoadSourceBuffer.limit(expectedCapacity);
                }
                int decompressedByteCount = decompressor.inflate(payLoadSourceBuffer.array());
                if ((payloadLength < int.MaxValue) && (decompressedByteCount < payloadLength))
                {
                    throw new ArgumentException("The buffer does not contain the indicated payload amount");
                }
            }

            histogram.fillCountsArrayFromSourceBuffer(
                    payLoadSourceBuffer,
                    expectedCapacity / getWordSizeInBytesFromCookie(cookie),
                    getWordSizeInBytesFromCookie(cookie));

            histogram.establishInternalTackingValues();

            return histogram;
        }

        private void fillCountsArrayFromSourceBuffer(ByteBuffer sourceBuffer, int lengthInWords, int wordSizeInBytes)
        {
            switch (wordSizeInBytes)
            {
                case 2:
                    {
                        ShortBuffer source = sourceBuffer.asShortBuffer();
                        for (int i = 0; i < lengthInWords; i++)
                        {
                            setCountAtIndex(i, source.get());
                        }
                        break;
                    }
                case 4:
                    {
                        IntBuffer source = sourceBuffer.asIntBuffer();
                        for (int i = 0; i < lengthInWords; i++)
                        {
                            setCountAtIndex(i, source.get());
                        }
                        break;
                    }
                case 8:
                    {
                        LongBuffer source = sourceBuffer.asLongBuffer();
                        for (int i = 0; i < lengthInWords; i++)
                        {
                            setCountAtIndex(i, source.get());
                        }
                        break;
                    }
                default:
                    throw new ArgumentException("word size must be 2, 4, or 8 bytes");
            }
        }

        internal static AbstractHistogram decodeFromCompressedByteBuffer(ByteBuffer buffer, Type histogramClass, long minBarForHighestTrackableValue)
        {
            int initialTargetPosition = buffer.position();
            int cookie = buffer.getInt();
            int headerSize;
            if (getCookieBase(cookie) == compressedEncodingCookieBase)
            {
                headerSize = ENCODING_HEADER_SIZE;
            }
            else if (getCookieBase(cookie) == V0EcompressedEncodingCookieBase)
            {
                headerSize = V0_ENCODING_HEADER_SIZE;
            }
            else
            {
                throw new ArgumentException("The buffer does not contain a compressed Histogram");
            }

            int lengthOfCompressedContents = buffer.getInt();
            Inflater decompressor = new Inflater();
            decompressor.setInput(buffer.array(), initialTargetPosition + 8, lengthOfCompressedContents);

            ByteBuffer headerBuffer = ByteBuffer.allocate(headerSize);
            decompressor.inflate(headerBuffer.array());
            AbstractHistogram histogram = decodeFromByteBuffer(
                    headerBuffer, histogramClass, minBarForHighestTrackableValue, decompressor, null);
            return histogram;
        }

        //
        //
        //
        // Internal helper methods:
        //
        //
        //

        void establishInternalTackingValues()
        {
            resetMaxValue(0);
            resetMinNonZeroValue(long.MaxValue);
            int maxIndex = -1;
            int minNonZeroIndex = -1;
            long observedTotalCount = 0;
            for (int index = 0; index < countsArrayLength; index++)
            {
                long countAtIndex;
                if ((countAtIndex = getCountAtIndex(index)) > 0)
                {
                    observedTotalCount += countAtIndex;
                    maxIndex = index;
                    if ((minNonZeroIndex == -1) && (index != 0))
                    {
                        minNonZeroIndex = index;
                    }
                }
            }
            if (maxIndex >= 0)
            {
                updatedMaxValue(highestEquivalentValue(valueFromIndex(maxIndex)));
            }
            if (minNonZeroIndex >= 0)
            {
                updateMinNonZeroValue(valueFromIndex(minNonZeroIndex));
            }
            setTotalCount(observedTotalCount);
        }

        internal int getBucketsNeededToCoverValue(long value)
        {
            long smallestUntrackableValue = ((long)subBucketCount) << unitMagnitude;
            int bucketsNeeded = 1;
            while (smallestUntrackableValue <= value)
            {
                if (smallestUntrackableValue > (long.MaxValue / 2))
                {
                    return bucketsNeeded + 1;
                }
                smallestUntrackableValue <<= 1;
                bucketsNeeded++;
            }
            return bucketsNeeded;
        }

        internal int getLengthForNumberOfBuckets(int numberOfBuckets)
        {
            int lengthNeeded = (numberOfBuckets + 1) * (subBucketCount / 2);
            return lengthNeeded;
        }

        protected int countsArrayIndex(long value)
        {
            if (value < 0)
            {
                throw new IndexOutOfRangeException("Histogram recorded value cannot be negative.");
            }
            int bucketIndex = getBucketIndex(value);
            int subBucketIndex = getSubBucketIndex(value, bucketIndex);
            return countsArrayIndex(bucketIndex, subBucketIndex);
        }

        private int countsArrayIndex(int bucketIndex, int subBucketIndex)
        {
            Debug.Assert(subBucketIndex < subBucketCount);
            Debug.Assert(bucketIndex == 0 || (subBucketIndex >= subBucketHalfCount));
            // Calculate the index for the first entry in the bucket:
            // (The following is the equivalent of ((bucketIndex + 1) * subBucketHalfCount) ):
            int bucketBaseIndex = (bucketIndex + 1) << subBucketHalfCountMagnitude;
            // Calculate the offset in the bucket (can be negative for first bucket):
            int offsetInBucket = subBucketIndex - subBucketHalfCount;
            // The following is the equivalent of ((subBucketIndex  - subBucketHalfCount) + bucketBaseIndex;
            return bucketBaseIndex + offsetInBucket;
        }

        int getBucketIndex(long value)
        {
            return leadingZeroCountBase - MathUtils.NumberOfLeadingZeros(value | subBucketMask);
        }

        int getSubBucketIndex(long value, int bucketIndex)
        {
            return (int)((ulong)value >> (bucketIndex + unitMagnitude));
        }

        protected int normalizeIndex(int index, int normalizingIndexOffset, int arrayLength)
        {
            if (normalizingIndexOffset == 0)
            {
                // Fastpath out of normalization. Keeps integer value histograms fast while allowing
                // others (like DoubleHistogram) to use normalization at a cost...
                return index;
            }
            if ((index > arrayLength) || (index < 0))
            {
                throw new IndexOutOfRangeException("index out of covered value range");
            }
            int normalizedIndex = index - normalizingIndexOffset;
            // The following is the same as an unsigned remainder operation, as long as no double wrapping happens
            // (which shouldn't happen, as normalization is never supposed to wrap, since it would have overflowed
            // or underflowed before it did). This (the + and - tests) seems to be faster than a % op with a
            // correcting if < 0...:
            if (normalizedIndex < 0)
            {
                normalizedIndex += arrayLength;
            }
            else if (normalizedIndex >= arrayLength)
            {
                normalizedIndex -= arrayLength;
            }
            return normalizedIndex;
        }

        long valueFromIndex(int bucketIndex, int subBucketIndex)
        {
            return ((long)subBucketIndex) << (bucketIndex + unitMagnitude);
        }

        internal long valueFromIndex(int index)
        {
            int bucketIndex = (index >> subBucketHalfCountMagnitude) - 1;
            int subBucketIndex = (index & (subBucketHalfCount - 1)) + subBucketHalfCount;
            if (bucketIndex < 0)
            {
                subBucketIndex -= subBucketHalfCount;
                bucketIndex = 0;
            }
            return valueFromIndex(bucketIndex, subBucketIndex);
        }

        internal static int numberOfSubbuckets(int numberOfSignificantValueDigits)
        {
            long largestValueWithSingleUnitResolution = 2 * (long)Math.Pow(10, numberOfSignificantValueDigits);

            // We need to maintain power-of-two subBucketCount (for clean direct indexing) that is large enough to
            // provide unit resolution to at least largestValueWithSingleUnitResolution. So figure out
            // largestValueWithSingleUnitResolution's nearest power-of-two (rounded up), and use that:
            int subBucketCountMagnitude = (int)Math.Ceiling(Math.Log(largestValueWithSingleUnitResolution) / Math.Log(2));
            int subBucketCount = (int)Math.Pow(2, subBucketCountMagnitude);
            return subBucketCount;
        }


    }

#pragma warning restore 0659
}


