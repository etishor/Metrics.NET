// Written by Gil Tene of Azul Systems, and released to the public domain,
// as explained at http://creativecommons.org/publicdomain/zero/1.0/
// 
// Ported to .NET by Iulian Margarintescu under the same license and terms as the java version
// Java Version repo: https://github.com/HdrHistogram/HdrHistogram
// Latest ported version is available in the Java submodule in the root of the repo

using System;
using System.Runtime.CompilerServices;
using Metrics.ConcurrencyUtilities;

namespace HdrHistogram
{
    /**
     * Records floating point (double) values, and provides stable
     * interval {@link DoubleHistogram} samples from live recorded data without interrupting or stalling active recording
     * of values. Each interval histogram provided contains all value counts accumulated since the
     * previous interval histogram was taken.
     * <p>
     * This pattern is commonly used in logging interval histogram information while recording is ongoing.
     * <p>
     * {@link DoubleRecorder} supports concurrent
     * {@link DoubleRecorder#RecordValue} or
     * {@link DoubleRecorder#RecordValueWithExpectedInterval} calls.
     * Recording calls are wait-free on architectures that support atomic increment operations, and
     * are lock-free on architectures that do not.
     *
     */

    public class DoubleRecorder
    {
        private static AtomicLong instanceIdSequencer = new AtomicLong(1);
        private long instanceId = instanceIdSequencer.GetAndIncrement();

        private WriterReaderPhaser recordingPhaser = new WriterReaderPhaser();

        private volatile InternalConcurrentDoubleHistogram activeHistogram;
        private InternalConcurrentDoubleHistogram inactiveHistogram;

        /**
         * Construct an auto-resizing {@link DoubleRecorder} using a precision stated as a number
         * of significant decimal digits.
         *
         * @param numberOfSignificantValueDigits Specifies the precision to use. This is the number of significant
         *                                       decimal digits to which the histogram will maintain value resolution
         *                                       and separation. Must be a non-negative integer between 0 and 5.
         */
        public DoubleRecorder(int numberOfSignificantValueDigits)
        {
            activeHistogram = new InternalConcurrentDoubleHistogram(instanceId, numberOfSignificantValueDigits);
            inactiveHistogram = new InternalConcurrentDoubleHistogram(instanceId, numberOfSignificantValueDigits);
            activeHistogram.setStartTimeStamp(Recorder.CurentTimeInMilis());
        }

        /**
         * Construct a {@link DoubleRecorder} dynamic range of values to cover and a number of significant
         * decimal digits.
         *
         * @param highestToLowestValueRatio specifies the dynamic range to use (as a ratio)
         * @param numberOfSignificantValueDigits Specifies the precision to use. This is the number of significant
         *                                       decimal digits to which the histogram will maintain value resolution
         *                                       and separation. Must be a non-negative integer between 0 and 5.
         */
        public DoubleRecorder(long highestToLowestValueRatio,
                              int numberOfSignificantValueDigits)
        {
            activeHistogram = new InternalConcurrentDoubleHistogram(
                    instanceId, highestToLowestValueRatio, numberOfSignificantValueDigits);
            inactiveHistogram = new InternalConcurrentDoubleHistogram(
                    instanceId, highestToLowestValueRatio, numberOfSignificantValueDigits);
            activeHistogram.setStartTimeStamp(Recorder.CurentTimeInMilis());
        }

        /**
         * Record a value
         * @param value the value to record
         * @throws ArrayIndexOutOfBoundsException (may throw) if value is exceeds highestTrackableValue
         */
        public void recordValue(double value)
        {
            long criticalValueAtEnter = recordingPhaser.WriterCriticalSectionEnter();
            try
            {
                activeHistogram.recordValue(value);
            }
            finally
            {
                recordingPhaser.WriterCriticalSectionExit(criticalValueAtEnter);
            }
        }

        /**
         * Record a value
         * <p>
         * To compensate for the loss of sampled values when a recorded value is larger than the expected
         * interval between value samples, Histogram will auto-generate an additional series of decreasingly-smaller
         * (down to the expectedIntervalBetweenValueSamples) value records.
         * <p>
         * See related notes {@link org.HdrHistogram.DoubleHistogram#RecordValueWithExpectedInterval(double, double)}
         * for more explanations about coordinated omission and expected interval correction.
         *      *
         * @param value The value to record
         * @param expectedIntervalBetweenValueSamples If expectedIntervalBetweenValueSamples is larger than 0, add
         *                                           auto-generated value records as appropriate if value is larger
         *                                           than expectedIntervalBetweenValueSamples
         * @throws ArrayIndexOutOfBoundsException (may throw) if value is exceeds highestTrackableValue
         */
        public void recordValueWithExpectedInterval(double value, double expectedIntervalBetweenValueSamples)
        {
            long criticalValueAtEnter = recordingPhaser.WriterCriticalSectionEnter();
            try
            {
                activeHistogram.recordValueWithExpectedInterval(value, expectedIntervalBetweenValueSamples);
            }
            finally
            {
                recordingPhaser.WriterCriticalSectionExit(criticalValueAtEnter);
            }
        }

        /**
         * Get a new instance of an interval histogram, which will include a stable, consistent view of all value
         * counts accumulated since the last interval histogram was taken.
         * <p>
         * Calling {@link DoubleRecorder#GetIntervalHistogram()} will reset
         * the value counts, and start accumulating value counts for the next interval.
         *
         * @return a histogram containing the value counts accumulated since the last interval histogram was taken.
         */
        [MethodImpl(MethodImplOptions.Synchronized)]
        public DoubleHistogram getIntervalHistogram()
        {
            return getIntervalHistogram(null);
        }

        /**
         * Get an interval histogram, which will include a stable, consistent view of all value counts
         * accumulated since the last interval histogram was taken.
         * <p>
         * {@link DoubleRecorder#GetIntervalHistogram(DoubleHistogram histogramToRecycle)
         * GetIntervalHistogram(histogramToRecycle)}
         * accepts a previously returned interval histogram that can be recycled internally to avoid allocation
         * and content copying operations, and is therefore significantly more efficient for repeated use than
         * {@link DoubleRecorder#GetIntervalHistogram()} and
         * {@link DoubleRecorder#getIntervalHistogramInto getIntervalHistogramInto()}. The provided
         * {@code histogramToRecycle} must
         * be either be null or an interval histogram returned by a previous call to
         * {@link DoubleRecorder#GetIntervalHistogram(DoubleHistogram histogramToRecycle)
         * GetIntervalHistogram(histogramToRecycle)} or
         * {@link DoubleRecorder#GetIntervalHistogram()}.
         * <p>
         * NOTE: The caller is responsible for not recycling the same returned interval histogram more than once. If
         * the same interval histogram instance is recycled more than once, behavior is undefined.
         * <p>
         * Calling {@link DoubleRecorder#GetIntervalHistogram(DoubleHistogram histogramToRecycle)
         * GetIntervalHistogram(histogramToRecycle)} will reset the value counts, and start accumulating value
         * counts for the next interval
         *
         * @param histogramToRecycle a previously returned interval histogram that may be recycled to avoid allocation and
         *                           copy operations.
         * @return a histogram containing the value counts accumulated since the last interval histogram was taken.
         */
        [MethodImpl(MethodImplOptions.Synchronized)]
        public DoubleHistogram getIntervalHistogram(DoubleHistogram histogramToRecycle)
        {
            if (histogramToRecycle == null)
            {
                histogramToRecycle = new InternalConcurrentDoubleHistogram(inactiveHistogram);
            }
            // Verify that replacement histogram can validly be used as an inactive histogram replacement:
            validateFitAsReplacementHistogram(histogramToRecycle);
            try
            {
                recordingPhaser.ReaderLock();
                inactiveHistogram = (InternalConcurrentDoubleHistogram)histogramToRecycle;
                performIntervalSample();
                return inactiveHistogram;
            }
            finally
            {
                recordingPhaser.ReaderUnlock();
            }
        }

        /**
         * Place a copy of the value counts accumulated since accumulated (since the last interval histogram
         * was taken) into {@code targetHistogram}.
         *
         * Calling {@link DoubleRecorder#getIntervalHistogramInto}() will reset
         * the value counts, and start accumulating value counts for the next interval.
         *
         * @param targetHistogram the histogram into which the interval histogram's data should be copied
         */
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void getIntervalHistogramInto(DoubleHistogram targetHistogram)
        {
            performIntervalSample();
            inactiveHistogram.copyInto(targetHistogram);
        }

        /**
         * Reset any value counts accumulated thus far.
         */
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void reset()
        {
            // the currently inactive histogram is reset each time we flip. So flipping twice resets both:
            performIntervalSample();
            performIntervalSample();
        }

        private void performIntervalSample()
        {
            inactiveHistogram.reset();
            try
            {
                recordingPhaser.ReaderLock();

                // Swap active and inactive histograms:
                InternalConcurrentDoubleHistogram tempHistogram = inactiveHistogram;
                inactiveHistogram = activeHistogram;
                activeHistogram = tempHistogram;

                // Mark end time of previous interval and start time of new one:
                long now = Recorder.CurentTimeInMilis();
                activeHistogram.setStartTimeStamp(now);
                inactiveHistogram.setEndTimeStamp(now);

                // Make sure we are not in the middle of recording a value on the previously active histogram:

                // Flip phase to make sure no recordings that were in flight pre-flip are still active:
                recordingPhaser.FlipPhase(500000L /* yield in 0.5 msec units if needed */);
            }
            finally
            {
                recordingPhaser.ReaderUnlock();
            }
        }

        private class InternalConcurrentDoubleHistogram : ConcurrentDoubleHistogram
        {
            public long containingInstanceId;

            public InternalConcurrentDoubleHistogram(long id, int numberOfSignificantValueDigits)
                : base(numberOfSignificantValueDigits)
            {
                this.containingInstanceId = id;
            }

            public InternalConcurrentDoubleHistogram(long id,
                                                      long highestToLowestValueRatio,
                                                      int numberOfSignificantValueDigits)
                : base(highestToLowestValueRatio, numberOfSignificantValueDigits)
            {
                this.containingInstanceId = id;
            }

            public InternalConcurrentDoubleHistogram(InternalConcurrentDoubleHistogram source)
                : base(source)
            {
                this.containingInstanceId = source.containingInstanceId;
            }
        }

        void validateFitAsReplacementHistogram(DoubleHistogram replacementHistogram)
        {
            var internalHistogram = replacementHistogram as InternalConcurrentDoubleHistogram;
            if (internalHistogram == null || internalHistogram.containingInstanceId != activeHistogram.containingInstanceId)
            {
                throw new ArgumentException("replacement histogram must have been obtained via a previous" +
                        "GetIntervalHistogram() call from this " + this.GetType().Name + " instance");
            }
        }
    }

}
