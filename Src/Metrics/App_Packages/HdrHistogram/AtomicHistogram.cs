// Written by Gil Tene of Azul Systems, and released to the public domain,
// as explained at http://creativecommons.org/publicdomain/zero/1.0/
// 
// Ported to .NET by Iulian Margarintescu under the same license and terms as the java version
// Java Version repo: https://github.com/HdrHistogram/HdrHistogram
// Latest ported version is available in the Java submodule in the root of the repo
using System;
using System.IO;
using System.Runtime.CompilerServices;
using HdrHistogram.ConcurrencyUtilities;

namespace HdrHistogram
{

    /**
     * <h3>A High Dynamic Range (HDR) Histogram using atomic <b><code>long</code></b> count type </h3>
     * An AtomicHistogram guarantees lossless recording of values into the histogram even when the
     * histogram is updated by multiple threads. It is important to note though that this lossless
     * recording capability is the only thread-safe behavior provided by AtomicHistogram, and that it
     * is not otherwise synchronized. Specifically, AtomicHistogram does not support auto-resizing,
     * does not support value shift operations, and provides no implicit synchronization
     * that would prevent the contents of the histogram from changing during iterations, copies, or
     * addition operations on the histogram. Callers wishing to make potentially concurrent,
     * multi-threaded updates that would safely work in the presence of queries, copies, or additions
     * of histogram objects should either take care to externally synchronize and/or order their access,
     * use the {@link org.HdrHistogram.SynchronizedHistogram} variant, or (recommended) use the
     * {@link Recorder} class, which is intended for this purpose.
     * <p>
     * See package description for {@link org.HdrHistogram} for details.
     */

    public class AtomicHistogram : Histogram
    {
        private new AtomicLong totalCount = new AtomicLong();
        private new AtomicLongArray counts;

        internal override long getCountAtIndex(int index)
        {
            return counts.GetValue(index);
        }

        protected override long getCountAtNormalizedIndex(int index)
        {
            return counts.GetValue(index);
        }

        protected override void incrementCountAtIndex(int index)
        {
            counts.GetAndIncrement(index);
        }

        protected override void addToCountAtIndex(int index, long value)
        {
            counts.GetAndAdd(index, value);
        }

        protected override void setCountAtIndex(int index, long value)
        {
            counts.SetValue(index, value);
        }

        protected override void setCountAtNormalizedIndex(int index, long value)
        {
            counts.SetValue(index, value);
        }

        protected override int getNormalizingIndexOffset()
        {
            return 0;
        }

        protected override void setNormalizingIndexOffset(int normalizingIndexOffset)
        {
            if (normalizingIndexOffset != 0)
            {
                throw new InvalidOperationException(
                    "AtomicHistogram does not support non-zero normalizing index settings." +
                    " Use ConcurrentHistogram Instead.");
            }
        }

        protected override void shiftNormalizingIndexByOffset(int offsetToAdd, bool lowestHalfBucketPopulated)
        {
            throw new InvalidOperationException(
                "AtomicHistogram does not support Shifting operations." +
                " Use ConcurrentHistogram Instead.");
        }

        protected internal override void resize(long newHighestTrackableValue)
        {
            throw new InvalidOperationException(
                "AtomicHistogram does not support resizing operations." +
                " Use ConcurrentHistogram Instead.");
        }

        protected internal override void clearCounts()
        {
            for (int i = 0; i < counts.Length; i++)
            {
                counts.SetValue(i, 0);
            }
            totalCount.SetValue(0);
        }

        public override AbstractHistogram copy()
        {
            AtomicHistogram copy = new AtomicHistogram(this);
            copy.add(this);
            return copy;
        }

        public override AbstractHistogram copyCorrectedForCoordinatedOmission(long expectedIntervalBetweenValueSamples)
        {
            AtomicHistogram toHistogram = new AtomicHistogram(this);
            toHistogram.addWhileCorrectingForCoordinatedOmission(this, expectedIntervalBetweenValueSamples);
            return toHistogram;
        }

        public override long getTotalCount()
        {
            return totalCount.GetValue();
        }

        protected override void setTotalCount(long totalCount)
        {
            this.totalCount.SetValue(totalCount);
        }

        protected override void incrementTotalCount()
        {
            this.totalCount.Increment();
        }

        protected override void addToTotalCount(long value)
        {
            this.totalCount.Add(value);
        }

        protected internal override int _getEstimatedFootprintInBytes()
        {
            return (512 + (8 * counts.Length));
        }

        /**
     * Construct a AtomicHistogram given the Highest value to be tracked and a number of significant decimal digits. The
     * histogram will be constructed to implicitly track (distinguish from 0) values as low as 1.
     *
     * @param highestTrackableValue The highest value to be tracked by the histogram. Must be a positive
     *                              integer that is {@literal >=} 2.
     * @param numberOfSignificantValueDigits Specifies the precision to use. This is the number of significant
     *                                       decimal digits to which the histogram will maintain value resolution
     *                                       and separation. Must be a non-negative integer between 0 and 5.
     */

        public AtomicHistogram(long highestTrackableValue, int numberOfSignificantValueDigits)
            : this(1, highestTrackableValue, numberOfSignificantValueDigits)
        {
        }

        /**
     * Construct a AtomicHistogram given the Lowest and Highest values to be tracked and a number of significant
     * decimal digits. Providing a lowestDiscernibleValue is useful is situations where the units used
     * for the histogram's values are much smaller that the minimal accuracy required. E.g. when tracking
     * time values stated in nanosecond units, where the minimal accuracy required is a microsecond, the
     * proper value for lowestDiscernibleValue would be 1000.
     *
     * @param lowestDiscernibleValue The lowest value that can be tracked (distinguished from 0) by the histogram.
     *                               Must be a positive integer that is {@literal >=} 1. May be internally rounded
     *                               down to nearest power of 2.
     * @param highestTrackableValue The highest value to be tracked by the histogram. Must be a positive
     *                              integer that is {@literal >=} (2 * lowestDiscernibleValue).
     * @param numberOfSignificantValueDigits Specifies the precision to use. This is the number of significant
     *                                       decimal digits to which the histogram will maintain value resolution
     *                                       and separation. Must be a non-negative integer between 0 and 5.
     */

        public AtomicHistogram(long lowestDiscernibleValue, long highestTrackableValue, int numberOfSignificantValueDigits)
            : base(lowestDiscernibleValue, highestTrackableValue, numberOfSignificantValueDigits, wordSizeInBytes: sizeof(long), allocateCountsArray: false, autoResize: false)
        {
            counts = new AtomicLongArray(countsArrayLength);
        }

        /**
     * Construct a histogram with the same range settings as a given source histogram,
     * duplicating the source's start/end timestamps (but NOT it's contents)
     * @param source The source histogram to duplicate
     */

        public AtomicHistogram(AbstractHistogram source)
            : base(source, false)
        {
            counts = new AtomicLongArray(countsArrayLength);
        }

        /**
     * Construct a new histogram by decoding it from a ByteBuffer.
     * @param buffer The buffer to decode from
     * @param minBarForHighestTrackableValue Force highestTrackableValue to be set at least this high
     * @return The newly constructed histogram
     */

        internal new static AtomicHistogram decodeFromByteBuffer(ByteBuffer buffer, long minBarForHighestTrackableValue)
        {
            return (AtomicHistogram)AbstractHistogram.decodeFromByteBuffer(buffer, typeof(AtomicHistogram), minBarForHighestTrackableValue);
        }

        /**
     * Construct a new histogram by decoding it from a compressed form in a ByteBuffer.
     * @param buffer The buffer to decode from
     * @param minBarForHighestTrackableValue Force highestTrackableValue to be set at least this high
     * @return The newly constructed histogram
     * @throws DataFormatException on error parsing/decompressing the buffer
     */

        public new static AtomicHistogram decodeFromCompressedByteBuffer(ByteBuffer buffer, long minBarForHighestTrackableValue)
        {
            return
                (AtomicHistogram)
                    decodeFromCompressedByteBuffer(buffer, typeof(AtomicHistogram), minBarForHighestTrackableValue);
        }

        private void readObject(Stream o)
        {
            // TODO: ??
            //o.defaultReadObject();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected internal override void fillCountsArrayFromBuffer(ByteBuffer buffer, int length)
        {
            LongBuffer logbuffer = buffer.asLongBuffer();
            for (int i = 0; i < length; i++)
            {
                counts.SetValue(i, logbuffer.get());
            }
        }

        // We try to cache the LongBuffer used in output cases, as repeated
        // output form the same histogram using the same buffer is likely:
        private LongBuffer cachedDstLongBuffer = null;
        private ByteBuffer cachedDstByteBuffer = null;
        private int cachedDstByteBufferPosition = 0;

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected internal override void fillBufferFromCountsArray(ByteBuffer buffer, int length)
        {
            if ((cachedDstLongBuffer == null) ||
                (buffer != cachedDstByteBuffer) ||
                (buffer.position() != cachedDstByteBufferPosition))
            {
                cachedDstByteBuffer = buffer;
                cachedDstByteBufferPosition = buffer.position();
                cachedDstLongBuffer = buffer.asLongBuffer();
            }
            cachedDstLongBuffer.rewind();
            int zeroIndex = normalizeIndex(0, getNormalizingIndexOffset(), countsArrayLength);
            int lengthFromZeroIndexToEnd = Math.Min(length, (countsArrayLength - zeroIndex));
            int remainingLengthFromNormalizedZeroIndex = length - lengthFromZeroIndexToEnd;
            for (int i = 0; i < lengthFromZeroIndexToEnd; i++)
            {
                cachedDstLongBuffer.put(counts.GetValue(zeroIndex + i));
            }
            for (int i = 0; i < remainingLengthFromNormalizedZeroIndex; i++)
            {
                cachedDstLongBuffer.put(counts.GetValue(i));
            }
        }
    }

}
