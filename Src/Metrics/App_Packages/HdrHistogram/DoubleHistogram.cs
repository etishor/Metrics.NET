// Written by Gil Tene of Azul Systems, and released to the public domain,
// as explained at http://creativecommons.org/publicdomain/zero/1.0/
// 
// Ported to .NET by Iulian Margarintescu under the same license and terms as the java version
// Java Version repo: https://github.com/HdrHistogram/HdrHistogram
// Latest ported version is available in the Java submodule in the root of the repo
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using HdrHistogram.ConcurrencyUtilities;

namespace HdrHistogram
{

    /**
     * <h3>A floating point values High Dynamic Range (HDR) Histogram</h3>
     * <p>
     * It is important to note that {@link DoubleHistogram} is not thread-safe, and does not support safe concurrent
     * recording by multiple threads. If concurrent operation is required, consider usings
     * {@link ConcurrentDoubleHistogram}, {@link SynchronizedDoubleHistogram}, or(recommended)
     * {@link DoubleRecorder}, which are intended for this purpose.
     * <p>
     * {@link DoubleHistogram} supports the recording and analyzing sampled data value counts across a
     * configurable dynamic range of floating point (double) values, with configurable value precision within the range.
     * Dynamic range is expressed as a ratio between the highest and lowest non-zero values trackable within the histogram
     * at any given time. Value precision is expressed as the number of significant [decimal] digits in the value recording,
     * and provides control over value quantization behavior across the value range and the subsequent value resolution at
     * any given level.
     * <p>
     * Auto-ranging: Unlike integer value based histograms, the specific value range tracked by a {@link
     * DoubleHistogram} is not specified upfront. Only the dynamic range of values that the histogram can cover is
     * (optionally) specified. E.g. When a {@link DoubleHistogram} is created to track a dynamic range of
     * 3600000000000 (enough to track values from a nanosecond to an hour), values could be recorded into into it in any
     * consistent unit of time as long as the ratio between the highest and lowest non-zero values stays within the
     * specified dynamic range, so recording in units of nanoseconds (1.0 thru 3600000000000.0), milliseconds (0.000001
     * thru 3600000.0) seconds (0.000000001 thru 3600.0), hours (1/3.6E12 thru 1.0) will all work just as well.
     * <p>
     * Auto-resizing: When constructed with no specified dynamic range (or when auto-resize is turned on with {@link
     * DoubleHistogram#setAutoResize}) a {@link DoubleHistogram} will auto-resize its dynamic range to
     * include recorded values as they are encountered. Note that recording calls that cause auto-resizing may take
     * longer to execute, as resizing incurs allocation and copying of internal data structures.
     * <p>
     * Attempts to record non-zero values that range outside of the specified dynamic range (or exceed the limits of
     * of dynamic range when auto-resizing) may results in {@link ArrayIndexOutOfBoundsException} exceptions, either
     * due to overflow or underflow conditions. These exceptions will only be thrown if recording the value would have
     * resulted in discarding or losing the required value precision of values already recorded in the histogram.
     * <p>
     * See package description for {@link org.HdrHistogram} for details.
     */

    public class DoubleHistogram : EncodableHistogram
    {
        private static readonly double highestAllowedValueEver; // A value that will keep us from multiplying into infinity.

        private long configuredHighestToLowestValueRatio;

        private VolatileDouble currentLowestValueInAutoRange = new VolatileDouble(0.0);
        private VolatileDouble currentHighestValueLimitInAutoRange = new VolatileDouble(0.0);

        internal AbstractHistogram integerValuesHistogram;

        internal VolatileDouble doubleToIntegerValueConversionRatio = new VolatileDouble(0.0);
        private VolatileDouble integerToDoubleValueConversionRatio = new VolatileDouble(0.0);

        private bool autoResize = false;

        /**
     * Construct a new auto-resizing DoubleHistogram using a precision stated as a number
     * of significant decimal digits.
     *
     * @param numberOfSignificantValueDigits Specifies the precision to use. This is the number of significant
     *                                       decimal digits to which the histogram will maintain value resolution
     *                                       and separation. Must be a non-negative integer between 0 and 5.
     */

        public DoubleHistogram(int numberOfSignificantValueDigits)
            : this(2, numberOfSignificantValueDigits, typeof(Histogram), null)
        {
            setAutoResize(true);
        }

        /**
     * Construct a new DoubleHistogram with the specified dynamic range (provided in
     * {@code highestToLowestValueRatio}) and using a precision stated as a number of significant
     * decimal digits.
     *
     * @param highestToLowestValueRatio specifies the dynamic range to use
     * @param numberOfSignificantValueDigits Specifies the precision to use. This is the number of significant
     *                                       decimal digits to which the histogram will maintain value resolution
     *                                       and separation. Must be a non-negative integer between 0 and 5.
     */

        public DoubleHistogram(long highestToLowestValueRatio, int numberOfSignificantValueDigits)
            : this(highestToLowestValueRatio, numberOfSignificantValueDigits, typeof(Histogram))
        {
        }

        /**
     * Construct a new DoubleHistogram with the specified dynamic range (provided in
     * {@code highestToLowestValueRatio}) and using a precision stated as a number of significant
     * decimal digits.
     *
     * The {@link org.HdrHistogram.DoubleHistogram} will use the specified AbstractHistogram subclass
     * for tracking internal counts (e.g. {@link org.HdrHistogram.Histogram},
     * {@link org.HdrHistogram.ConcurrentHistogram}, {@link org.HdrHistogram.SynchronizedHistogram},
     * {@link org.HdrHistogram.IntCountsHistogram}, {@link org.HdrHistogram.ShortCountsHistogram}).
     *
     * @param highestToLowestValueRatio specifies the dynamic range to use.
     * @param numberOfSignificantValueDigits Specifies the precision to use. This is the number of significant
     *                                       decimal digits to which the histogram will maintain value resolution
     *                                       and separation. Must be a non-negative integer between 0 and 5.
     * @param internalCountsHistogramClass The class to use for internal counts tracking
     */

        internal protected DoubleHistogram(long highestToLowestValueRatio,
            int numberOfSignificantValueDigits,
            Type internalCountsHistogramClass)
            : this(highestToLowestValueRatio, numberOfSignificantValueDigits, internalCountsHistogramClass, null)
        {
        }

        private DoubleHistogram(long highestToLowestValueRatio,
            int numberOfSignificantValueDigits,
            Type internalCountsHistogramClass,
            AbstractHistogram internalCountsHistogram)
            : this(
                highestToLowestValueRatio,
                numberOfSignificantValueDigits,
                internalCountsHistogramClass,
                internalCountsHistogram,
                false
                )
        {
        }

        private DoubleHistogram(long highestToLowestValueRatio,
            int numberOfSignificantValueDigits,
            Type internalCountsHistogramClass,
            AbstractHistogram internalCountsHistogram,
            bool mimicInternalModel)
        {

            if (highestToLowestValueRatio < 2)
            {
                throw new ArgumentException("highestToLowestValueRatio must be >= 2");
            }

            if ((highestToLowestValueRatio * Math.Pow(10.0, numberOfSignificantValueDigits)) >= (1L << 61))
            {
                throw new ArgumentException(
                    "highestToLowestValueRatio * (10^numberOfSignificantValueDigits) must be < (1L << 61)");
            }
            if (internalCountsHistogramClass == typeof(AtomicHistogram))
            {
                throw new ArgumentException(
                    "AtomicHistogram cannot be used as an internal counts histogram (does not support shifting)." +
                    " Use ConcurrentHistogram instead.");
            }

            try
            {

                long integerValueRange = deriveIntegerValueRange(highestToLowestValueRatio, numberOfSignificantValueDigits);

                AbstractHistogram valuesHistogram;
                double initialLowestValueInAutoRange;

                if (internalCountsHistogram == null)
                {
                    var histogramConstructor = internalCountsHistogramClass.GetConstructor(new[] { typeof(long), typeof(long), typeof(int) });
                    valuesHistogram = histogramConstructor.Invoke(new object[] { 1L, (integerValueRange - 1), numberOfSignificantValueDigits })
                        as AbstractHistogram;

                    // We want the auto-ranging to tend towards using a value range that will result in using the
                    // lower tracked value ranges and leave the higher end empty unless the range is actually used.
                    // This is most easily done by making early recordings force-shift the lower value limit to
                    // accommodate them (forcing a force-shift for the higher values would achieve the opposite).
                    // We will therefore start with a very high value range, and let the recordings autoAdjust
                    // downwards from there:
                    initialLowestValueInAutoRange = Math.Pow(2.0, 800);
                }
                else if (mimicInternalModel)
                {
                    var histogramConstructor = internalCountsHistogramClass.GetConstructor(new[] { typeof(AbstractHistogram) });
                    valuesHistogram = histogramConstructor.Invoke(new object[] { internalCountsHistogram })
                        as AbstractHistogram;

                    initialLowestValueInAutoRange = Math.Pow(2.0, 800);
                }
                else
                {
                    // Verify that the histogram we got matches:
                    if ((internalCountsHistogram.getLowestDiscernibleValue() != 1) ||
                        (internalCountsHistogram.getHighestTrackableValue() != integerValueRange - 1) ||
                        internalCountsHistogram.getNumberOfSignificantValueDigits() != numberOfSignificantValueDigits)
                    {
                        throw new InvalidOperationException("integer values histogram does not match stated parameters.");
                    }
                    valuesHistogram = internalCountsHistogram;
                    // Derive initialLowestValueInAutoRange from valuesHistogram's integerToDoubleValueConversionRatio:
                    initialLowestValueInAutoRange =
                        internalCountsHistogram.integerToDoubleValueConversionRatio *
                        internalCountsHistogram.subBucketHalfCount;
                }

                // Set our double tracking range and internal histogram:
                init(highestToLowestValueRatio, initialLowestValueInAutoRange, valuesHistogram);
            }
            catch (TargetInvocationException ex)
            {
                throw new ArgumentException("internalCountsHistogramClass", ex);
            }
        }

        /**
     * Construct a {@link org.HdrHistogram.DoubleHistogram} with the same range settings as a given source,
     * duplicating the source's start/end timestamps (but NOT it's contents)
     * @param source The source histogram to duplicate
     */

        public DoubleHistogram(DoubleHistogram source)
            : this(source.configuredHighestToLowestValueRatio,
                source.getNumberOfSignificantValueDigits(),
                source.integerValuesHistogram.GetType(),
                source.integerValuesHistogram,
                true)
        {
            this.autoResize = source.autoResize;
        }

        private void init(long configuredHighestToLowestValueRatio, double lowestTrackableUnitValue, AbstractHistogram integerValuesHistogram)
        {
            this.configuredHighestToLowestValueRatio = configuredHighestToLowestValueRatio;
            this.integerValuesHistogram = integerValuesHistogram;
            long internalHighestToLowestValueRatio =
                deriveInternalHighestToLowestValueRatio(configuredHighestToLowestValueRatio);
            setTrackableValueRange(lowestTrackableUnitValue, lowestTrackableUnitValue * internalHighestToLowestValueRatio);
        }

        private void setTrackableValueRange(double lowestValueInAutoRange, double highestValueInAutoRange)
        {
            this.currentLowestValueInAutoRange.SetValue(lowestValueInAutoRange);
            this.currentHighestValueLimitInAutoRange.SetValue(highestValueInAutoRange);
            this.integerToDoubleValueConversionRatio.SetValue(lowestValueInAutoRange / getLowestTrackingIntegerValue());
            this.doubleToIntegerValueConversionRatio.SetValue(1.0 / integerToDoubleValueConversionRatio.GetValue());
            integerValuesHistogram.integerToDoubleValueConversionRatio = integerToDoubleValueConversionRatio.GetValue();
        }

        //
        //
        // Auto-resizing control:
        //
        //

        public bool isAutoResize()
        {
            return autoResize;
        }

        public void setAutoResize(bool autoResize)
        {
            this.autoResize = autoResize;
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
     * @throws ArrayIndexOutOfBoundsException (may throw) if value is cannot be covered by the histogram's range
     */

        public void recordValue(double value)
        {
            recordSingleValue(value);
        }

        /**
     * Record a value in the histogram (adding to the value's current count)
     *
     * @param value The value to be recorded
     * @param count The number of occurrences of this value to record
     * @throws ArrayIndexOutOfBoundsException (may throw) if value is cannot be covered by the histogram's range
     */

        public void recordValueWithCount(double value, long count)
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
     * by {@link #copyCorrectedForCoordinatedOmission(double)}.
     * The use cases for these two methods are mutually exclusive, and only one of the two should be be used on
     * a given data set to correct for the same coordinated omission issue.
     * <p>
     * See notes in the description of the Histogram calls for an illustration of why this corrective behavior is
     * important.
     *
     * @param value The value to record
     * @param expectedIntervalBetweenValueSamples If expectedIntervalBetweenValueSamples is larger than 0, add
     *                                           auto-generated value records as appropriate if value is larger
     *                                           than expectedIntervalBetweenValueSamples
     * @throws ArrayIndexOutOfBoundsException (may throw) if value is cannot be covered by the histogram's range
     */

        public void recordValueWithExpectedInterval(double value, double expectedIntervalBetweenValueSamples)
        {
            recordValueWithCountAndExpectedInterval(value, 1, expectedIntervalBetweenValueSamples);
        }

        private void recordCountAtValue(long count, double value)
        {
            if ((value < currentLowestValueInAutoRange.GetValue()) || (value > currentHighestValueLimitInAutoRange.GetValue()))
            {
                // Zero is valid and needs no auto-ranging, but also rare enough that we should deal
                // with it on the slow path...
                autoAdjustRangeForValue(value);
            }

            long integerValue = (long)(value * doubleToIntegerValueConversionRatio.GetValue());
            integerValuesHistogram.recordValueWithCount(integerValue, count);
        }

        private void recordSingleValue(double value)
        {
            if ((value < currentLowestValueInAutoRange.GetValue()) || (value >= currentHighestValueLimitInAutoRange.GetValue()))
            {
                // Zero is valid and needs no auto-ranging, but also rare enough that we should deal
                // with it on the slow path...
                autoAdjustRangeForValue(value);
            }

            long integerValue = (long)(value * doubleToIntegerValueConversionRatio.GetValue());
            integerValuesHistogram.recordValue(integerValue);
        }

        private void recordValueWithCountAndExpectedInterval(double value, long count, double expectedIntervalBetweenValueSamples)
        {
            recordCountAtValue(count, value);
            if (expectedIntervalBetweenValueSamples <= 0)
                return;
            for (double missingValue = value - expectedIntervalBetweenValueSamples;
                missingValue >= expectedIntervalBetweenValueSamples;
                missingValue -= expectedIntervalBetweenValueSamples)
            {
                recordCountAtValue(count, missingValue);
            }
        }

        //
        //
        //
        // Shift and auto-ranging support:
        //
        //
        //

        private void autoAdjustRangeForValue(double value)
        {
            // Zero is always valid, and doesn't need auto-range adjustment:
            if (value == 0.0)
            {
                return;
            }
            autoAdjustRangeForValueSlowPath(value);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void autoAdjustRangeForValueSlowPath(double value)
        {
            if (value < currentLowestValueInAutoRange.GetValue())
            {
                if (value < 0.0)
                {
                    throw new IndexOutOfRangeException("Negative values cannot be recorded");
                }
                do
                {
                    int shiftAmount =
                        findCappedContainingBinaryOrderOfMagnitude(
                            Math.Ceiling(currentLowestValueInAutoRange.GetValue() / value) - 1.0);
                    shiftCoveredRangeToTheRight(shiftAmount);
                } while (value < currentLowestValueInAutoRange.GetValue());
            }
            else if (value >= currentHighestValueLimitInAutoRange.GetValue())
            {
                if (value > highestAllowedValueEver)
                {
                    throw new IndexOutOfRangeException("Values above " + highestAllowedValueEver + " cannot be recorded");
                }
                do
                {
                    // If value is an exact whole multiple of currentHighestValueLimitInAutoRange, it "belongs" with
                    // the next level up, as it crosses the limit. With floating point values, the simplest way to
                    // make this shift on exact multiple values happen (but not for any just-smaller-than-exact-multiple
                    // values) is to use a value that is 1 ulp bigger in computing the ratio for the shift amount:
                    int shiftAmount =
                        findCappedContainingBinaryOrderOfMagnitude(
                            Math.Ceiling((value + MathUtils.ULP(value)) / currentHighestValueLimitInAutoRange.GetValue()) - 1.0);
                    shiftCoveredRangeToTheLeft(shiftAmount);
                } while (value >= currentHighestValueLimitInAutoRange.GetValue());
            }
        }

        private void shiftCoveredRangeToTheRight(int numberOfBinaryOrdersOfMagnitude)
        {
            // We are going to adjust the tracked range by effectively shifting it to the right
            // (in the integer shift sense).
            //
            // To counter the right shift of the value multipliers, we need to left shift the internal
            // representation such that the newly shifted integer values will continue to return the
            // same double values.

            // Initially, new range is the same as current range, to make sure we correctly recover
            // from a shift failure if one happens:
            double newLowestValueInAutoRange = currentLowestValueInAutoRange.GetValue();
            double newHighestValueLimitInAutoRange = currentHighestValueLimitInAutoRange.GetValue();

            try
            {
                double shiftMultiplier = 1.0 / (1L << numberOfBinaryOrdersOfMagnitude);

                // First, temporarily change the highest value in auto-range without changing conversion ratios.
                // This is done to force new values higher than the new expected highest value to attempt an
                // adjustment (which is synchronized and will wait behind this one). This ensures that we will
                // not end up with any concurrently recorded values that would need to be discarded if the shift
                // fails. If this shift succeeds, the pending adjustment attempt will end up doing nothing.
                currentHighestValueLimitInAutoRange.SetValue(currentHighestValueLimitInAutoRange.GetValue() * shiftMultiplier);

                // First shift the values, to give the shift a chance to fail:

                // Shift integer histogram left, increasing the recorded integer values for current recordings
                // by a factor of (1 << numberOfBinaryOrdersOfMagnitude):

                // (no need to shift any values if all recorded values are at the 0 value level:)
                if (getTotalCount() > integerValuesHistogram.getCountAtIndex(0))
                {
                    // Apply the shift:
                    try
                    {
                        integerValuesHistogram.shiftValuesLeft(numberOfBinaryOrdersOfMagnitude);
                    }
                    catch (IndexOutOfRangeException ex)
                    {
                        // Failed to shift, try to expand size instead:
                        handleShiftValuesException(numberOfBinaryOrdersOfMagnitude, ex);
                        // First expand the highest limit to reflect successful size expansion:
                        newHighestValueLimitInAutoRange /= shiftMultiplier;
                        // Successfully expanded histogram range by numberOfBinaryOrdersOfMagnitude, but not
                        // by shifting (shifting failed because there was not room to shift left into). Instead,
                        // we grew the max value without changing the value mapping. Since we were trying to
                        // shift values left to begin with, trying to shift the left again will work (we now
                        // have room to shift into):
                        integerValuesHistogram.shiftValuesLeft(numberOfBinaryOrdersOfMagnitude);
                    }
                }
                // Shift (or resize) was successful. Adjust new range to reflect:
                newLowestValueInAutoRange *= shiftMultiplier;
                newHighestValueLimitInAutoRange *= shiftMultiplier;
            }
            finally
            {
                // Set the new range to either the successfully changed one, or the original one:
                setTrackableValueRange(newLowestValueInAutoRange, newHighestValueLimitInAutoRange);
            }
        }

        private void shiftCoveredRangeToTheLeft(int numberOfBinaryOrdersOfMagnitude)
        {
            // We are going to adjust the tracked range by effectively shifting it to the right
            // (in the integer shift sense).
            //
            // To counter the left shift of the value multipliers, we need to right shift the internal
            // representation such that the newly shifted integer values will continue to return the
            // same double values.

            // Initially, new range is the same as current range, to make sure we correctly recover
            // from a shift failure if one happens:
            double newLowestValueInAutoRange = currentLowestValueInAutoRange.GetValue();
            double newHighestValueLimitInAutoRange = currentHighestValueLimitInAutoRange.GetValue();

            try
            {
                double shiftMultiplier = 1.0 * (1L << numberOfBinaryOrdersOfMagnitude);

                // First, temporarily change the lowest value in auto-range without changing conversion ratios.
                // This is done to force new values lower than the new expected lowest value to attempt an
                // adjustment (which is synchronized and will wait behind this one). This ensures that we will
                // not end up with any concurrently recorded values that would need to be discarded if the shift
                // fails. If this shift succeeds, the pending adjustment attempt will end up doing nothing.
                currentLowestValueInAutoRange.SetValue(currentLowestValueInAutoRange.GetValue() * shiftMultiplier);

                // First shift the values, to give the shift a chance to fail:

                // Shift integer histogram right, decreasing the recorded integer values for current recordings
                // by a factor of (1 << numberOfBinaryOrdersOfMagnitude):

                // (no need to shift any values if all recorded values are at the 0 value level:)
                if (getTotalCount() > integerValuesHistogram.getCountAtIndex(0))
                {
                    // Apply the shift:
                    try
                    {
                        integerValuesHistogram.shiftValuesRight(numberOfBinaryOrdersOfMagnitude);
                        // Shift was successful. Adjust new range to reflect:
                        newLowestValueInAutoRange *= shiftMultiplier;
                        newHighestValueLimitInAutoRange *= shiftMultiplier;
                    }
                    catch (IndexOutOfRangeException ex)
                    {
                        // Failed to shift, try to expand size instead:
                        handleShiftValuesException(numberOfBinaryOrdersOfMagnitude, ex);
                        // Successfully expanded histogram range by numberOfBinaryOrdersOfMagnitude, but not
                        // by shifting (shifting failed because there was not room to shift right into). Instead,
                        // we grew the max value without changing the value mapping. Since we were trying to
                        // shift values right to begin with to make room for a larger value than we had had
                        // been able to fit before, no shift is needed, as the value should now fit. So rather
                        // than shifting and adjusting both lowest and highest limits, we'll end up just
                        // expanding newHighestValueLimitInAutoRange to indicate the newly expanded range.
                        // We therefore reverse-scale the newLowestValueInAutoRange before lating the later
                        // code scale both up:
                        newLowestValueInAutoRange /= shiftMultiplier;
                    }
                }
                // Shift (or resize) was successful. Adjust new range to reflect:
                newLowestValueInAutoRange *= shiftMultiplier;
                newHighestValueLimitInAutoRange *= shiftMultiplier;
            }
            finally
            {
                // Set the new range to either the successfully changed one, or the original one:
                setTrackableValueRange(newLowestValueInAutoRange, newHighestValueLimitInAutoRange);
            }
        }

        private void handleShiftValuesException(int numberOfBinaryOrdersOfMagnitude, Exception ex)
        {
            if (!autoResize)
            {
                throw new IndexOutOfRangeException("value outside of histogram covered range. Caused by: " + ex);
            }

            long highestTrackableValue = integerValuesHistogram.getHighestTrackableValue();
            int highestTrackableValueContainingOrderOfMagnitude =
                findContainingBinaryOrderOfMagnitude(highestTrackableValue);
            long newHighestTrackableValue =
                (1L << (numberOfBinaryOrdersOfMagnitude + highestTrackableValueContainingOrderOfMagnitude)) - 1;
            if (newHighestTrackableValue < highestTrackableValue)
            {
                throw new IndexOutOfRangeException(
                    "cannot resize histogram covered range beyond (1L << 63) / (1L << " +
                    (integerValuesHistogram.subBucketHalfCountMagnitude) + ") - 1.\n" +
                    "Caused by:" + ex);
            }
            integerValuesHistogram.resize(newHighestTrackableValue);
            integerValuesHistogram.highestTrackableValue = newHighestTrackableValue;
            configuredHighestToLowestValueRatio <<= numberOfBinaryOrdersOfMagnitude;
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
            integerValuesHistogram.clearCounts();
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

        public DoubleHistogram copy()
        {
            DoubleHistogram targetHistogram =
                new DoubleHistogram(configuredHighestToLowestValueRatio, getNumberOfSignificantValueDigits());
            targetHistogram.setTrackableValueRange(currentLowestValueInAutoRange.GetValue(), currentHighestValueLimitInAutoRange.GetValue());
            integerValuesHistogram.copyInto(targetHistogram.integerValuesHistogram);
            return targetHistogram;
        }

        /**
     * Get a copy of this histogram, corrected for coordinated omission.
     * <p>
     * To compensate for the loss of sampled values when a recorded value is larger than the expected
     * interval between value samples, the new histogram will include an auto-generated additional series of
     * decreasingly-smaller (down to the expectedIntervalBetweenValueSamples) value records for each count found
     * in the current histogram that is larger than the expectedIntervalBetweenValueSamples.
     *
     * Note: This is a post-correction method, as opposed to the at-recording correction method provided
     * by {@link #recordValueWithExpectedInterval(double, double) recordValueWithExpectedInterval}. The two
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

        public DoubleHistogram copyCorrectedForCoordinatedOmission(double expectedIntervalBetweenValueSamples)
        {
            DoubleHistogram targetHistogram =
                new DoubleHistogram(configuredHighestToLowestValueRatio, getNumberOfSignificantValueDigits());
            targetHistogram.setTrackableValueRange(currentLowestValueInAutoRange.GetValue(), currentHighestValueLimitInAutoRange.GetValue());
            targetHistogram.addWhileCorrectingForCoordinatedOmission(this, expectedIntervalBetweenValueSamples);
            return targetHistogram;
        }

        /**
     * Copy this histogram into the target histogram, overwriting it's contents.
     *
     * @param targetHistogram the histogram to copy into
     */

        public void copyInto(DoubleHistogram targetHistogram)
        {
            targetHistogram.reset();
            targetHistogram.add(this);
            targetHistogram.setStartTimeStamp(integerValuesHistogram.startTimeStampMsec);
            targetHistogram.setEndTimeStamp(integerValuesHistogram.endTimeStampMsec);
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

        public void copyIntoCorrectedForCoordinatedOmission(DoubleHistogram targetHistogram,
            double expectedIntervalBetweenValueSamples)
        {
            targetHistogram.reset();
            targetHistogram.addWhileCorrectingForCoordinatedOmission(this, expectedIntervalBetweenValueSamples);
            targetHistogram.setStartTimeStamp(integerValuesHistogram.startTimeStampMsec);
            targetHistogram.setEndTimeStamp(integerValuesHistogram.endTimeStampMsec);
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
     *
     * @param fromHistogram The other histogram.
     * @throws ArrayIndexOutOfBoundsException (may throw) if values in fromHistogram's cannot be
     * covered by this histogram's range
     */

        public void add(DoubleHistogram fromHistogram)
        {
            int arrayLength = fromHistogram.integerValuesHistogram.countsArrayLength;
            AbstractHistogram fromIntegerHistogram = fromHistogram.integerValuesHistogram;
            for (int i = 0; i < arrayLength; i++)
            {
                long count = fromIntegerHistogram.getCountAtIndex(i);
                if (count > 0)
                {
                    recordValueWithCount(
                        fromIntegerHistogram.valueFromIndex(i) *
                        fromHistogram.integerToDoubleValueConversionRatio.GetValue(),
                        count);
                }
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
     * by {@link #recordValueWithExpectedInterval(double, double) recordValueWithExpectedInterval}. The two
     * methods are mutually exclusive, and only one of the two should be be used on a given data set to correct
     * for the same coordinated omission issue.
     * by
     * <p>
     * See notes in the description of the Histogram calls for an illustration of why this corrective behavior is
     * important.
     *
     * @param fromHistogram Other histogram. highestToLowestValueRatio and numberOfSignificantValueDigits must match.
     * @param expectedIntervalBetweenValueSamples If expectedIntervalBetweenValueSamples is larger than 0, add
     *                                           auto-generated value records as appropriate if value is larger
     *                                           than expectedIntervalBetweenValueSamples
     * @throws ArrayIndexOutOfBoundsException (may throw) if values exceed highestTrackableValue
     */

        public void addWhileCorrectingForCoordinatedOmission(DoubleHistogram fromHistogram,
            double expectedIntervalBetweenValueSamples)
        {
            DoubleHistogram toHistogram = this;

            foreach (HistogramIterationValue v in fromHistogram.integerValuesHistogram.RecordedValues())
            {
                toHistogram.recordValueWithCountAndExpectedInterval(
                    v.getValueIteratedTo() * integerToDoubleValueConversionRatio.GetValue(),
                    v.getCountAtValueIteratedTo(), expectedIntervalBetweenValueSamples);
            }
        }

        /**
     * Subtract the contents of another histogram from this one.
     *
     * @param otherHistogram The other histogram.
     * @throws ArrayIndexOutOfBoundsException (may throw) if values in fromHistogram's cannot be
     * covered by this histogram's range
     */

        public void subtract(DoubleHistogram otherHistogram)
        {
            int arrayLength = otherHistogram.integerValuesHistogram.countsArrayLength;
            AbstractHistogram otherIntegerHistogram = otherHistogram.integerValuesHistogram;
            for (int i = 0; i < arrayLength; i++)
            {
                long otherCount = otherIntegerHistogram.getCountAtIndex(i);
                if (otherCount > 0)
                {
                    double otherValue = otherIntegerHistogram.valueFromIndex(i) *
                                        otherHistogram.integerToDoubleValueConversionRatio.GetValue();
                    if (getCountAtValue(otherValue) < otherCount)
                    {
                        throw new ArgumentException("otherHistogram count (" + otherCount + ") at value " +
                                                    otherValue + " is larger than this one's (" + getCountAtValue(otherValue) + ")");
                    }
                    recordValueWithCount(otherValue, -otherCount);
                }
            }
        }

        //
        //
        //
        // Comparison support:
        //
        //
        //

        public override int GetHashCode()
        {
            // compiler warns if equals is implemented without GetHashCode()
            return integerValuesHistogram.GetHashCode();
        }

        /**
     * Determine if this histogram is equivalent to another.
     *
     * @param other the other histogram to compare to
     * @return True if this histogram are equivalent with the other.
     */

        public override bool Equals(object other)
        {
            if (this == other)
            {
                return true;
            }
            if (!(other is DoubleHistogram))
            {
                return false;
            }
            DoubleHistogram that = (DoubleHistogram)other;
            if ((currentLowestValueInAutoRange.GetValue() != that.currentLowestValueInAutoRange.GetValue()) ||
                (currentHighestValueLimitInAutoRange.GetValue() != that.currentHighestValueLimitInAutoRange.GetValue()) ||
                (getNumberOfSignificantValueDigits() != that.getNumberOfSignificantValueDigits()))
            {
                return false;
            }
            if (integerValuesHistogram.countsArrayLength != that.integerValuesHistogram.countsArrayLength)
            {
                return false;
            }
            if (getTotalCount() != that.getTotalCount())
            {
                return false;
            }
            for (int i = 0; i < integerValuesHistogram.countsArrayLength; i++)
            {
                if (integerValuesHistogram.getCountAtIndex(i) != that.integerValuesHistogram.getCountAtIndex(i))
                {
                    return false;
                }
            }
            return true;
        }

        //
        //
        //
        // Histogram structure querying support:
        //
        //
        //

        /**
     * Get the total count of all recorded values in the histogram
     * @return the total count of all recorded values in the histogram
     */

        public long getTotalCount()
        {
            return integerValuesHistogram.getTotalCount();
        }

        /**
     * get the current lowest (non zero) trackable value the automatically determined range
     * (keep in mind that this can change because it is auto ranging)
     * @return current lowest trackable value the automatically determined range
     */

        internal double getCurrentLowestTrackableNonZeroValue()
        {
            return currentLowestValueInAutoRange.GetValue();
        }

        /**
     * get the current highest trackable value in the automatically determined range
     * (keep in mind that this can change because it is auto ranging)
     * @return current highest trackable value in the automatically determined range
     */

        internal double getCurrentHighestTrackableValue()
        {
            return currentHighestValueLimitInAutoRange.GetValue();
        }

        /**
     * Get the current conversion ratio from interval integer value representation to double units.
     * (keep in mind that this can change because it is auto ranging). This ratio can be useful
     * for converting integer values found in iteration, although the preferred form for accessing
     * iteration values would be to use the
     * {@link org.HdrHistogram.HistogramIterationValue#getDoubleValueIteratedTo() getDoubleValueIteratedTo()}
     * and
     * {@link org.HdrHistogram.HistogramIterationValue#getDoubleValueIteratedFrom() getDoubleValueIteratedFrom()}
     * accessors to {@link org.HdrHistogram.HistogramIterationValue} iterated values.
     *
     * @return the current conversion ratio from interval integer value representation to double units.
     */

        public double getIntegerToDoubleValueConversionRatio()
        {
            return integerToDoubleValueConversionRatio.GetValue();
        }

        /**
     * get the configured numberOfSignificantValueDigits
     * @return numberOfSignificantValueDigits
     */

        public int getNumberOfSignificantValueDigits()
        {
            return integerValuesHistogram.NumberOfSignificantValueDigits;
        }

        /**
     * get the Dynamic range of the histogram: the configured ratio between the highest trackable value and the
     * lowest trackable non zero value at any given time.
     * @return the dynamic range of the histogram, expressed as the ratio between the highest trackable value
     * and the lowest trackable non zero value at any given time.
     */

        public long getHighestToLowestValueRatio()
        {
            return configuredHighestToLowestValueRatio;
        }

        /**
     * Get the size (in value units) of the range of values that are equivalent to the given value within the
     * histogram's resolution. Where "equivalent" means that value samples recorded for any two
     * equivalent values are counted in a common total count.
     *
     * @param value The given value
     * @return The lowest value that is equivalent to the given value within the histogram's resolution.
     */

        public double sizeOfEquivalentValueRange(double value)
        {
            return integerValuesHistogram.sizeOfEquivalentValueRange((long)(value * doubleToIntegerValueConversionRatio.GetValue())) *
                   integerToDoubleValueConversionRatio.GetValue();
        }

        /**
     * Get the lowest value that is equivalent to the given value within the histogram's resolution.
     * Where "equivalent" means that value samples recorded for any two
     * equivalent values are counted in a common total count.
     *
     * @param value The given value
     * @return The lowest value that is equivalent to the given value within the histogram's resolution.
     */

        public double lowestEquivalentValue(double value)
        {
            return integerValuesHistogram.lowestEquivalentValue((long)(value * doubleToIntegerValueConversionRatio.GetValue())) *
                   integerToDoubleValueConversionRatio.GetValue();
        }

        /**
     * Get the highest value that is equivalent to the given value within the histogram's resolution.
     * Where "equivalent" means that value samples recorded for any two
     * equivalent values are counted in a common total count.
     *
     * @param value The given value
     * @return The highest value that is equivalent to the given value within the histogram's resolution.
     */

        public double highestEquivalentValue(double value)
        {
            double nextNonEquivalent = nextNonEquivalentValue(value);
            // Theoretically, nextNonEquivalentValue - ulp(nextNonEquivalentValue) == nextNonEquivalentValue
            // is possible (if the ulp size switches right at nextNonEquivalentValue), so drop by 2 ulps and
            // increment back up to closest within-ulp value.
            double highestEquivalentValue = nextNonEquivalent - (2 * MathUtils.ULP(nextNonEquivalent));
            while (highestEquivalentValue + MathUtils.ULP(highestEquivalentValue) < nextNonEquivalent)
            {
                highestEquivalentValue += MathUtils.ULP(highestEquivalentValue);
            }

            return highestEquivalentValue;
        }

        /**
     * Get a value that lies in the middle (rounded up) of the range of values equivalent the given value.
     * Where "equivalent" means that value samples recorded for any two
     * equivalent values are counted in a common total count.
     *
     * @param value The given value
     * @return The value lies in the middle (rounded up) of the range of values equivalent the given value.
     */

        public double medianEquivalentValue(double value)
        {
            return integerValuesHistogram.medianEquivalentValue((long)(value * doubleToIntegerValueConversionRatio.GetValue())) *
                   integerToDoubleValueConversionRatio.GetValue();
        }

        /**
     * Get the next value that is not equivalent to the given value within the histogram's resolution.
     * Where "equivalent" means that value samples recorded for any two
     * equivalent values are counted in a common total count.
     *
     * @param value The given value
     * @return The next value that is not equivalent to the given value within the histogram's resolution.
     */

        public double nextNonEquivalentValue(double value)
        {
            return integerValuesHistogram.nextNonEquivalentValue((long)(value * doubleToIntegerValueConversionRatio.GetValue())) *
                   integerToDoubleValueConversionRatio.GetValue();
        }

        /**
     * Determine if two values are equivalent with the histogram's resolution.
     * Where "equivalent" means that value samples recorded for any two
     * equivalent values are counted in a common total count.
     *
     * @param value1 first value to compare
     * @param value2 second value to compare
     * @return True if values are equivalent to within the histogram's resolution.
     */

        public bool valuesAreEquivalent(double value1, double value2)
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
            return integerValuesHistogram._getEstimatedFootprintInBytes();
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
            return integerValuesHistogram.startTimeStampMsec;
        }

        /**
     * Set the start time stamp value associated with this histogram to a given value.
     * @param timeStampMsec the value to set the time stamp to, [by convention] in msec since the epoch.
     */

        public override void setStartTimeStamp(long timeStampMsec)
        {
            this.integerValuesHistogram.startTimeStampMsec = timeStampMsec;
        }

        /**
     * get the end time stamp [optionally] stored with this histogram
     * @return the end time stamp [optionally] stored with this histogram
     */

        public override long getEndTimeStamp()
        {
            return integerValuesHistogram.endTimeStampMsec;
        }

        /**
     * Set the end time stamp value associated with this histogram to a given value.
     * @param timeStampMsec the value to set the time stamp to, [by convention] in msec since the epoch.
     */

        public override void setEndTimeStamp(long timeStampMsec)
        {
            this.integerValuesHistogram.endTimeStampMsec = timeStampMsec;
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

        public double getMinValue()
        {
            return integerValuesHistogram.getMinValue() * integerToDoubleValueConversionRatio.GetValue();
        }

        /**
     * Get the highest recorded value level in the histogram
     *
     * @return the Max value recorded in the histogram
     */

        public double getMaxValue()
        {
            return integerValuesHistogram.getMaxValue() * integerToDoubleValueConversionRatio.GetValue();
        }

        /**
     * Get the lowest recorded non-zero value level in the histogram
     *
     * @return the lowest recorded non-zero value level in the histogram
     */

        public double getMinNonZeroValue()
        {
            return integerValuesHistogram.getMinNonZeroValue() * integerToDoubleValueConversionRatio.GetValue();
        }

        /**
     * Get the highest recorded value level in the histogram as a double
     *
     * @return the highest recorded value level in the histogram as a double
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
            return integerValuesHistogram.getMean() * integerToDoubleValueConversionRatio.GetValue();
        }

        /**
     * Get the computed standard deviation of all recorded values in the histogram
     *
     * @return the standard deviation (in value units) of the histogram data
     */

        public double getStdDeviation()
        {
            return integerValuesHistogram.getStdDeviation() * integerToDoubleValueConversionRatio.GetValue();
        }

        /**
     * Get the value at a given percentile.
     * When the percentile is &gt; 0.0, the value returned is the value that the given the given
     * percentage of the overall recorded value entries in the histogram are either smaller than
     * or equivalent to. When the percentile is 0.0, the value returned is the value that all value
     * entries in the histogram are either larger than or equivalent to.
     * <p>
     * Note that two values are "equivalent" in this statement if
     * {@link org.HdrHistogram.DoubleHistogram#valuesAreEquivalent} would return true.
     *
     * @param percentile  The percentile for which to return the associated value
     * @return The value that the given percentage of the overall recorded value entries in the
     * histogram are either smaller than or equivalent to. When the percentile is 0.0, returns the
     * value that all value entries in the histogram are either larger than or equivalent to.
     */

        public double getValueAtPercentile(double percentile)
        {
            return integerValuesHistogram.getValueAtPercentile(percentile) * integerToDoubleValueConversionRatio.GetValue();
        }

        /**
     * Get the percentile at a given value.
     * The percentile returned is the percentile of values recorded in the histogram that are smaller
     * than or equivalent to the given value.
     * <p>
     * Note that two values are "equivalent" in this statement if
     * {@link org.HdrHistogram.DoubleHistogram#valuesAreEquivalent} would return true.
     *
     * @param value The value for which to return the associated percentile
     * @return The percentile of values recorded in the histogram that are smaller than or equivalent
     * to the given value.
     */

        public double getPercentileAtOrBelowValue(double value)
        {
            return integerValuesHistogram.getPercentileAtOrBelowValue((long)(value * doubleToIntegerValueConversionRatio.GetValue()));
        }

        /**
     * Get the count of recorded values within a range of value levels (inclusive to within the histogram's resolution).
     *
     * @param lowValue  The lower value bound on the range for which
     *                  to provide the recorded count. Will be rounded down with
     *                  {@link DoubleHistogram#lowestEquivalentValue lowestEquivalentValue}.
     * @param highValue  The higher value bound on the range for which to provide the recorded count.
     *                   Will be rounded up with {@link DoubleHistogram#highestEquivalentValue highestEquivalentValue}.
     * @return the total count of values recorded in the histogram within the value range that is
     * {@literal >=} lowestEquivalentValue(<i>lowValue</i>) and {@literal <=} highestEquivalentValue(<i>highValue</i>)
     */

        public double getCountBetweenValues(double lowValue, double highValue)
        {
            return integerValuesHistogram.getCountBetweenValues(
                (long)(lowValue * doubleToIntegerValueConversionRatio.GetValue()),
                (long)(highValue * doubleToIntegerValueConversionRatio.GetValue())
                );
        }

        /**
     * Get the count of recorded values at a specific value (to within the histogram resolution at the value level).
     *
     * @param value The value for which to provide the recorded count
     * @return The total count of values recorded in the histogram within the value range that is
     * {@literal >=} lowestEquivalentValue(<i>value</i>) and {@literal <=} highestEquivalentValue(<i>value</i>)
     */

        public long getCountAtValue(double value)
        {
            return integerValuesHistogram.getCountAtValue((long)(value * doubleToIntegerValueConversionRatio.GetValue()));
        }

        /**
     * Provide a means of iterating through histogram values according to percentile levels. The iteration is
     * performed in steps that start at 0% and reduce their distance to 100% according to the
     * <i>percentileTicksPerHalfDistance</i> parameter, ultimately reaching 100% when all recorded histogram
     * values are exhausted.
     * <p>
     * @param percentileTicksPerHalfDistance The number of iteration steps per half-distance to 100%.
     * @return An {@link java.lang.Iterable}{@literal <}{@link DoubleHistogramIterationValue}{@literal >}
     * through the histogram using a
     * {@link DoublePercentileIterator}
     */

        public Percentiles percentiles(int percentileTicksPerHalfDistance)
        {
            return new Percentiles(this, percentileTicksPerHalfDistance);
        }

        /**
     * Provide a means of iterating through histogram values using linear steps. The iteration is
     * performed in steps of <i>valueUnitsPerBucket</i> in size, terminating when all recorded histogram
     * values are exhausted.
     *
     * @param valueUnitsPerBucket  The size (in value units) of the linear buckets to use
     * @return An {@link java.lang.Iterable}{@literal <}{@link DoubleHistogramIterationValue}{@literal >}
     * through the histogram using a
     * {@link DoubleLinearIterator}
     */

        public LinearBucketValues linearBucketValues(double valueUnitsPerBucket)
        {
            return new LinearBucketValues(this, valueUnitsPerBucket);
        }

        /**
     * Provide a means of iterating through histogram values at logarithmically increasing levels. The iteration is
     * performed in steps that start at <i>valueUnitsInFirstBucket</i> and increase exponentially according to
     * <i>logBase</i>, terminating when all recorded histogram values are exhausted.
     *
     * @param valueUnitsInFirstBucket The size (in value units) of the first bucket in the iteration
     * @param logBase The multiplier by which bucket sizes will grow in each iteration step
     * @return An {@link java.lang.Iterable}{@literal <}{@link DoubleHistogramIterationValue}{@literal >}
     * through the histogram using
     * a {@link DoubleLogarithmicIterator}
     */

        public LogarithmicBucketValues logarithmicBucketValues(double valueUnitsInFirstBucket,
            double logBase)
        {
            return new LogarithmicBucketValues(this, valueUnitsInFirstBucket, logBase);
        }

        /**
     * Provide a means of iterating through all recorded histogram values using the finest granularity steps
     * supported by the underlying representation. The iteration steps through all non-zero recorded value counts,
     * and terminates when all recorded histogram values are exhausted.
     *
     * @return An {@link java.lang.Iterable}{@literal <}{@link DoubleHistogramIterationValue}{@literal >}
     * through the histogram using
     * a {@link DoubleRecordedValuesIterator}
     */

        public RecordedValues recordedValues()
        {
            return new RecordedValues(this);
        }

        /**
     * Provide a means of iterating through all histogram values using the finest granularity steps supported by
     * the underlying representation. The iteration steps through all possible unit value levels, regardless of
     * whether or not there were recorded values for that value level, and terminates when all recorded histogram
     * values are exhausted.
     *
     * @return An {@link java.lang.Iterable}{@literal <}{@link DoubleHistogramIterationValue}{@literal >}
     * through the histogram using a {@link DoubleAllValuesIterator}
     */

        public AllValues allValues()
        {
            return new AllValues(this);
        }


        // Percentile iterator support:

        /**
     * An {@link java.lang.Iterable}{@literal <}{@link DoubleHistogramIterationValue}{@literal >} through
     * the histogram using a {@link DoublePercentileIterator}
     */

        public class Percentiles : Iterable<DoubleHistogramIterationValue>
        {
            private DoubleHistogram histogram;
            private int percentileTicksPerHalfDistance;

            public Percentiles(DoubleHistogram histogram, int percentileTicksPerHalfDistance)
            {
                this.histogram = histogram;
                this.percentileTicksPerHalfDistance = percentileTicksPerHalfDistance;
            }

            /**
         * @return A {@link DoublePercentileIterator}{@literal <}{@link DoubleHistogramIterationValue}{@literal >}
         */

            protected override Iterator<DoubleHistogramIterationValue> iterator()
            {
                return new DoublePercentileIterator(histogram, percentileTicksPerHalfDistance);
            }
        }

        // Linear iterator support:

        /**
     * An {@link java.lang.Iterable}{@literal <}{@link DoubleHistogramIterationValue}{@literal >} through
     * the histogram using a {@link DoubleLinearIterator}
     */

        public class LinearBucketValues : Iterable<DoubleHistogramIterationValue>
        {
            private DoubleHistogram histogram;
            private double valueUnitsPerBucket;

            public LinearBucketValues(DoubleHistogram histogram, double valueUnitsPerBucket)
            {
                this.histogram = histogram;
                this.valueUnitsPerBucket = valueUnitsPerBucket;
            }

            /**
         * @return A {@link DoubleLinearIterator}{@literal <}{@link DoubleHistogramIterationValue}{@literal >}
         */

            protected override Iterator<DoubleHistogramIterationValue> iterator()
            {
                return new DoubleLinearIterator(histogram, valueUnitsPerBucket);
            }
        }

        // Logarithmic iterator support:

        /**
     * An {@link java.lang.Iterable}{@literal <}{@link DoubleHistogramIterationValue}{@literal >} through
     * the histogram using a {@link DoubleLogarithmicIterator}
     */

        public class LogarithmicBucketValues : Iterable<DoubleHistogramIterationValue>
        {
            private DoubleHistogram histogram;
            private double valueUnitsInFirstBucket;
            private double logBase;

            public LogarithmicBucketValues(DoubleHistogram histogram,
                double valueUnitsInFirstBucket, double logBase)
            {
                this.histogram = histogram;
                this.valueUnitsInFirstBucket = valueUnitsInFirstBucket;
                this.logBase = logBase;
            }

            /**
         * @return A {@link DoubleLogarithmicIterator}{@literal <}{@link DoubleHistogramIterationValue}{@literal >}
         */

            protected override Iterator<DoubleHistogramIterationValue> iterator()
            {
                return new DoubleLogarithmicIterator(histogram, valueUnitsInFirstBucket, logBase);
            }
        }

        // Recorded value iterator support:

        /**
     * An {@link java.lang.Iterable}{@literal <}{@link DoubleHistogramIterationValue}{@literal >} through
     * the histogram using a {@link DoubleRecordedValuesIterator}
     */

        public class RecordedValues : Iterable<DoubleHistogramIterationValue>
        {
            private DoubleHistogram histogram;

            public RecordedValues(DoubleHistogram histogram)
            {
                this.histogram = histogram;
            }

            /**
         * @return A {@link DoubleRecordedValuesIterator}{@literal <}{@link HistogramIterationValue}{@literal >}
         */

            protected override Iterator<DoubleHistogramIterationValue> iterator()
            {
                return new DoubleRecordedValuesIterator(histogram);
            }
        }

        // AllValues iterator support:

        /**
     * An {@link java.lang.Iterable}{@literal <}{@link DoubleHistogramIterationValue}{@literal >} through
     * the histogram using a {@link DoubleAllValuesIterator}
     */

        public class AllValues : Iterable<DoubleHistogramIterationValue>
        {
            private DoubleHistogram histogram;

            public AllValues(DoubleHistogram histogram)
            {
                this.histogram = histogram;
            }

            /**
         * @return A {@link DoubleAllValuesIterator}{@literal <}{@link HistogramIterationValue}{@literal >}
         */

            protected override Iterator<DoubleHistogramIterationValue> iterator()
            {
                return new DoubleAllValuesIterator(histogram);
            }
        }



        /**
     * Produce textual representation of the value distribution of histogram data by percentile. The distribution is
     * output with exponentially increasing resolution, with each exponentially decreasing half-distance containing
     * five (5) percentile reporting tick points.
     *
     * @param printStream    Stream into which the distribution will be output
     * <p>
     * @param outputValueUnitScalingRatio    The scaling factor by which to divide histogram recorded values units in
     *                                     output
     */

        public void outputPercentileDistribution(TextWriter printStream,
            Double outputValueUnitScalingRatio)
        {
            outputPercentileDistribution(printStream, 5, outputValueUnitScalingRatio);
        }

        //
        //
        //
        // Textual percentile output support:
        //
        //
        //

        /**
     * Produce textual representation of the value distribution of histogram data by percentile. The distribution is
     * output with exponentially increasing resolution, with each exponentially decreasing half-distance containing
     * <i>dumpTicksPerHalf</i> percentile reporting tick points.
     *
     * @param printStream    Stream into which the distribution will be output
     * <p>
     * @param percentileTicksPerHalfDistance  The number of reporting points per exponentially decreasing half-distance
     * <p>
     * @param outputValueUnitScalingRatio    The scaling factor by which to divide histogram recorded values units in
     *                                     output
     */

        public void outputPercentileDistribution(TextWriter printStream,
            int percentileTicksPerHalfDistance,
            Double outputValueUnitScalingRatio)
        {
            outputPercentileDistribution(printStream, percentileTicksPerHalfDistance, outputValueUnitScalingRatio, false);
        }

        /**
     * Produce textual representation of the value distribution of histogram data by percentile. The distribution is
     * output with exponentially increasing resolution, with each exponentially decreasing half-distance containing
     * <i>dumpTicksPerHalf</i> percentile reporting tick points.
     *
     * @param printStream    Stream into which the distribution will be output
     * <p>
     * @param percentileTicksPerHalfDistance  The number of reporting points per exponentially decreasing half-distance
     * <p>
     * @param outputValueUnitScalingRatio    The scaling factor by which to divide histogram recorded values units in
     *                                     output
     * @param useCsvFormat  Output in CSV format if true. Otherwise use plain text form.
     */

        public void outputPercentileDistribution(TextWriter printStream,
            int percentileTicksPerHalfDistance,
            Double outputValueUnitScalingRatio,
            bool useCsvFormat)
        {
            integerValuesHistogram.OutputPercentileDistribution(printStream,
                percentileTicksPerHalfDistance,
                outputValueUnitScalingRatio / integerToDoubleValueConversionRatio.GetValue(),
                useCsvFormat);
        }

        //
        //
        //
        // Serialization support:
        //
        //
        //

        //private static long serialVersionUID = 42L;

        private void writeObject(BinaryWriter o)
        {
            o.Write(configuredHighestToLowestValueRatio);
            o.Write(currentLowestValueInAutoRange.GetValue());
            integerValuesHistogram.writeObject(o);
        }

        private void readObject(BinaryReader o)
        {
            long configuredHighestToLowestValueRatio = o.ReadInt64();
            double lowestValueInAutoRange = o.ReadDouble();
            // TODO: serialization support
            //AbstractHistogram integerValuesHistogram = (AbstractHistogram)o.readObject();
            //init(configuredHighestToLowestValueRatio, lowestValueInAutoRange, integerValuesHistogram);
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

        public override int getNeededByteBufferCapacity()
        {
            return integerValuesHistogram.getNeededByteBufferCapacity();
        }

        private int getNeededByteBufferCapacity(int relevantLength)
        {
            return integerValuesHistogram.getNeededByteBufferCapacity(relevantLength);
        }

        private void fillCountsArrayFromBuffer(ByteBuffer buffer, int length)
        {
            integerValuesHistogram.fillCountsArrayFromBuffer(buffer, length);
        }

        private void fillBufferFromCountsArray(ByteBuffer buffer, int length)
        {
            integerValuesHistogram.fillBufferFromCountsArray(buffer, length);
        }

        private static int DHIST_encodingCookie = 0x0c72124e;
        private static int DHIST_compressedEncodingCookie = 0x0c72124f;

        private static bool isDoubleHistogramCookie(int cookie)
        {
            return isCompressedDoubleHistogramCookie(cookie) || isNonCompressedDoubleHistogramCookie(cookie);
        }

        private static bool isCompressedDoubleHistogramCookie(int cookie)
        {
            return (cookie == DHIST_compressedEncodingCookie);
        }

        private static bool isNonCompressedDoubleHistogramCookie(int cookie)
        {
            return (cookie == DHIST_encodingCookie);
        }

        /**
     * Encode this histogram into a ByteBuffer
     * @param buffer The buffer to encode into
     * @return The number of bytes written to the buffer
     */

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int encodeIntoByteBuffer(ByteBuffer buffer)
        {
            long maxValue = integerValuesHistogram.getMaxValue();
            int relevantLength = integerValuesHistogram.getLengthForNumberOfBuckets(
                integerValuesHistogram.getBucketsNeededToCoverValue(maxValue));
            if (buffer.capacity() < getNeededByteBufferCapacity(relevantLength))
            {
                throw new IndexOutOfRangeException("buffer does not have capacity for" +
                                                   getNeededByteBufferCapacity(relevantLength) + " bytes");
            }
            buffer.putInt(DHIST_encodingCookie);
            buffer.putInt(getNumberOfSignificantValueDigits());
            buffer.putLong(configuredHighestToLowestValueRatio);
            return integerValuesHistogram.encodeIntoByteBuffer(buffer) + 16;
        }

        /**
     * Encode this histogram in compressed form into a byte array
     * @param targetBuffer The buffer to encode into
     * @param compressionLevel Compression level (for java.util.zip.Deflater).
     * @return The number of bytes written to the buffer
     */

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override int encodeIntoCompressedByteBuffer(
            ByteBuffer targetBuffer,
            int compressionLevel)
        {
            targetBuffer.putInt(DHIST_compressedEncodingCookie);
            targetBuffer.putInt(getNumberOfSignificantValueDigits());
            targetBuffer.putLong(configuredHighestToLowestValueRatio);
            return integerValuesHistogram.encodeIntoCompressedByteBuffer(targetBuffer, compressionLevel) + 16;
        }

        /**
     * Encode this histogram in compressed form into a byte array
     * @param targetBuffer The buffer to encode into
     * @return The number of bytes written to the array
     */

        public int encodeIntoCompressedByteBuffer(ByteBuffer targetBuffer)
        {
            return encodeIntoCompressedByteBuffer(targetBuffer, Deflater.DEFAULT_COMPRESSION);
        }

        private static DoubleHistogram constructHistogramFromBuffer(
            int cookie,
            ByteBuffer buffer,
            Type histogramClass,
            long minBarForHighestToLowestValueRatio)
        {
            int numberOfSignificantValueDigits = buffer.getInt();
            long configuredHighestToLowestValueRatio = buffer.getLong();
            AbstractHistogram valuesHistogram;
            if (isNonCompressedDoubleHistogramCookie(cookie))
            {
                valuesHistogram =
                    AbstractHistogram.decodeFromByteBuffer(buffer, histogramClass, minBarForHighestToLowestValueRatio);
            }
            else if (isCompressedDoubleHistogramCookie(cookie))
            {
                valuesHistogram =
                    AbstractHistogram.decodeFromCompressedByteBuffer(buffer, histogramClass, minBarForHighestToLowestValueRatio);
            }
            else
            {
                throw new InvalidOperationException("The buffer does not contain a DoubleHistogram");
            }
            DoubleHistogram histogram =
                new DoubleHistogram(
                    configuredHighestToLowestValueRatio,
                    numberOfSignificantValueDigits,
                    histogramClass,
                    valuesHistogram
                    );
            return histogram;
        }

        /**
     * Construct a new DoubleHistogram by decoding it from a ByteBuffer.
     * @param buffer The buffer to decode from
     * @param minBarForHighestToLowestValueRatio Force highestTrackableValue to be set at least this high
     * @return The newly constructed DoubleHistogram
     */

        public static DoubleHistogram decodeFromByteBuffer(
            ByteBuffer buffer,
            long minBarForHighestToLowestValueRatio)
        {
            return decodeFromByteBuffer(buffer, typeof(Histogram), minBarForHighestToLowestValueRatio);
        }

        /**
     * Construct a new DoubleHistogram by decoding it from a ByteBuffer, using a
     * specified AbstractHistogram subclass for tracking internal counts (e.g. {@link org.HdrHistogram.Histogram},
     * {@link org.HdrHistogram.ConcurrentHistogram}, {@link org.HdrHistogram.SynchronizedHistogram},
     * {@link org.HdrHistogram.IntCountsHistogram}, {@link org.HdrHistogram.ShortCountsHistogram}).
     *
     * @param buffer The buffer to decode from
     * @param internalCountsHistogramClass The class to use for internal counts tracking
     * @param minBarForHighestToLowestValueRatio Force highestTrackableValue to be set at least this high
     * @return The newly constructed DoubleHistogram
     */

        public static DoubleHistogram decodeFromByteBuffer(
            ByteBuffer buffer,
            Type internalCountsHistogramClass,
            long minBarForHighestToLowestValueRatio)
        {

            int cookie = buffer.getInt();
            if (!isNonCompressedDoubleHistogramCookie(cookie))
            {
                throw new ArgumentException("The buffer does not contain a DoubleHistogram");
            }
            DoubleHistogram histogram = constructHistogramFromBuffer(cookie, buffer, internalCountsHistogramClass,
                minBarForHighestToLowestValueRatio);
            return histogram;
        }

        /**
     * Construct a new DoubleHistogram by decoding it from a compressed form in a ByteBuffer.
     * @param buffer The buffer to decode from
     * @param minBarForHighestToLowestValueRatio Force highestTrackableValue to be set at least this high
     * @return The newly constructed DoubleHistogram
     * @throws DataFormatException on error parsing/decompressing the buffer
     */

        public static DoubleHistogram decodeFromCompressedByteBuffer(
            ByteBuffer buffer,
            long minBarForHighestToLowestValueRatio)
        {
            return decodeFromCompressedByteBuffer(buffer, typeof(Histogram), minBarForHighestToLowestValueRatio);
        }

        /**
     * Construct a new DoubleHistogram by decoding it from a compressed form in a ByteBuffer, using a
     * specified AbstractHistogram subclass for tracking internal counts (e.g. {@link org.HdrHistogram.Histogram},
     * {@link org.HdrHistogram.AtomicHistogram}, {@link org.HdrHistogram.SynchronizedHistogram},
     * {@link org.HdrHistogram.IntCountsHistogram}, {@link org.HdrHistogram.ShortCountsHistogram}).
     *
     * @param buffer The buffer to decode from
     * @param internalCountsHistogramClass The class to use for internal counts tracking
     * @param minBarForHighestToLowestValueRatio Force highestTrackableValue to be set at least this high
     * @return The newly constructed DoubleHistogram
     * @throws DataFormatException on error parsing/decompressing the buffer
     */

        public static DoubleHistogram decodeFromCompressedByteBuffer(
            ByteBuffer buffer,
            Type internalCountsHistogramClass,
            long minBarForHighestToLowestValueRatio)
        {
            int cookie = buffer.getInt();
            if (!isCompressedDoubleHistogramCookie(cookie))
            {
                throw new ArgumentException("The buffer does not contain a compressed DoubleHistogram");
            }
            DoubleHistogram histogram = constructHistogramFromBuffer(cookie, buffer, internalCountsHistogramClass,
                minBarForHighestToLowestValueRatio);
            return histogram;
        }

        //
        //
        //
        // Internal helper methods:
        //
        //
        //

        private long deriveInternalHighestToLowestValueRatio(long externalHighestToLowestValueRatio)
        {
            // Internal dynamic range needs to be 1 order of magnitude larger than the containing order of magnitude.
            // e.g. the dynamic range that covers [0.9, 2.1) is 2.33x, which on it's own would require 4x range to
            // cover the contained order of magnitude. But (if 1.0 was a bucket boundary, for example, the range
            // will actually need to cover [0.5..1.0) [1.0..2.0) [2.0..4.0), mapping to an 8x internal dynamic range.
            long internalHighestToLowestValueRatio =
                1L << (findContainingBinaryOrderOfMagnitude(externalHighestToLowestValueRatio) + 1);
            return internalHighestToLowestValueRatio;
        }

        private long deriveIntegerValueRange(long externalHighestToLowestValueRatio,
            int numberOfSignificantValueDigits)
        {
            long internalHighestToLowestValueRatio =
                deriveInternalHighestToLowestValueRatio(externalHighestToLowestValueRatio);

            // We cannot use the bottom half of bucket 0 in an integer values histogram to represent double
            // values, because the required precision does not exist there. We therefore need the integer
            // range to be bigger, such that the entire double value range can fit in the upper halves of
            // all buckets. Compute the integer value range that will achieve this:

            long lowestTackingIntegerValue = AbstractHistogram.numberOfSubbuckets(numberOfSignificantValueDigits) / 2;
            long integerValueRange = lowestTackingIntegerValue * internalHighestToLowestValueRatio;

            return integerValueRange;
        }

        private long getLowestTrackingIntegerValue()
        {
            return integerValuesHistogram.subBucketHalfCount;
        }

        private static int findContainingBinaryOrderOfMagnitude(long longNumber)
        {
            int pow2ceiling = 64 - MathUtils.NumberOfLeadingZeros(longNumber); // smallest power of 2 containing value
            return pow2ceiling;
        }

        private static int findContainingBinaryOrderOfMagnitude(double doubleNumber)
        {
            long longNumber = (long)Math.Ceiling(doubleNumber);
            return findContainingBinaryOrderOfMagnitude(longNumber);
        }

        private int findCappedContainingBinaryOrderOfMagnitude(double doubleNumber)
        {
            if (doubleNumber > configuredHighestToLowestValueRatio)
            {
                return (int)(Math.Log(configuredHighestToLowestValueRatio) / Math.Log(2));
            }
            if (doubleNumber > Math.Pow(2.0, 50))
            {
                return 50;
            }
            return findContainingBinaryOrderOfMagnitude(doubleNumber);
        }

        static DoubleHistogram()
        {
            // We don't want to allow the histogram to shift and expand into value ranges that could equate
            // to infinity (e.g. 1024.0 * (Double.MAX_VALUE / 1024.0) == Infinity). So lets makes sure the
            // highestAllowedValueEver cap is a couple of bindary orders of magnitude away from MAX_VALUE:

            // Choose a highestAllowedValueEver that is a nice power of 2 multiple of 1.0 :
            double value = 1.0;
            while (value < Double.MaxValue / 4.0)
            {
                value *= 2;
            }
            highestAllowedValueEver = value;
        }
    }
}
