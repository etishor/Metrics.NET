// Written by Gil Tene of Azul Systems, and released to the public domain,
// as explained at http://creativecommons.org/publicdomain/zero/1.0/
// 
// Ported to .NET by Iulian Margarintescu under the same license and terms as the java version
// Java Version repo: https://github.com/HdrHistogram/HdrHistogram
// Latest ported version is available in the Java submodule in the root of the repo
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace HdrHistogram
{
    /**
  * <h3>A High Dynamic Range (HDR) Histogram using a <b><code>short</code></b> count type </h3>
  * <p>
  * See package description for {@link org.HdrHistogram} for details.
  */

    public class ShortCountsHistogram : AbstractHistogram
    {
        long totalCount;
        short[] counts;
        int normalizingIndexOffset;

        internal override long getCountAtIndex(int index)
        {
            return counts[normalizeIndex(index, normalizingIndexOffset, countsArrayLength)];
        }

        protected override long getCountAtNormalizedIndex(int index)
        {
            return counts[index];
        }

        protected override void incrementCountAtIndex(int index)
        {
            int normalizedIndex = normalizeIndex(index, normalizingIndexOffset, countsArrayLength);
            short currentCount = counts[normalizedIndex];
            short newCount = (short)(currentCount + 1);
            if (newCount < 0)
            {
                throw new InvalidOperationException("would overflow short integer count");
            }
            counts[normalizedIndex] = newCount;
        }

        protected override void addToCountAtIndex(int index, long value)
        {
            int normalizedIndex = normalizeIndex(index, normalizingIndexOffset, countsArrayLength);
            short currentCount = counts[normalizedIndex];
            if ((value < 0) || (value > short.MaxValue))
            {
                throw new ArgumentException("would overflow short integer count");
            }
            short newCount = (short)(currentCount + value);
            if (newCount < 0)
            {
                throw new InvalidOperationException("would overflow short integer count");
            }
            counts[normalizedIndex] = newCount;
        }

        protected override void setCountAtIndex(int index, long value)
        {
            setCountAtNormalizedIndex(normalizeIndex(index, normalizingIndexOffset, countsArrayLength), value);
        }

        protected override void setCountAtNormalizedIndex(int index, long value)
        {
            if ((value < 0) || (value > short.MaxValue))
            {
                throw new ArgumentException("would overflow short integer count");
            }
            counts[index] = (short)value;
        }

        protected override int getNormalizingIndexOffset()
        {
            return normalizingIndexOffset;
        }

        protected override void setNormalizingIndexOffset(int normalizingIndexOffset)
        {
            this.normalizingIndexOffset = normalizingIndexOffset;
        }

        protected override void shiftNormalizingIndexByOffset(int offsetToAdd, bool lowestHalfBucketPopulated)
        {
            nonConcurrentNormalizingIndexShift(offsetToAdd, lowestHalfBucketPopulated);
        }

        protected internal override void clearCounts()
        {
            Array.Clear(counts, 0, counts.Length);
            //java.util.Arrays.fill(counts, (short)0);
            totalCount = 0;
        }

        public override AbstractHistogram copy()
        {
            ShortCountsHistogram copy = new ShortCountsHistogram(this);
            copy.add(this);
            return copy;
        }

        public override AbstractHistogram copyCorrectedForCoordinatedOmission(long expectedIntervalBetweenValueSamples)
        {
            ShortCountsHistogram toHistogram = new ShortCountsHistogram(this);
            toHistogram.addWhileCorrectingForCoordinatedOmission(this, expectedIntervalBetweenValueSamples);
            return toHistogram;
        }

        public override long getTotalCount()
        {
            return totalCount;
        }

        protected override void setTotalCount(long totalCount)
        {
            this.totalCount = totalCount;
        }

        protected override void incrementTotalCount()
        {
            totalCount++;
        }

        protected override void addToTotalCount(long value)
        {
            totalCount += value;
        }

        protected internal override int _getEstimatedFootprintInBytes()
        {
            return (512 + (2 * counts.Length));
        }

        protected internal override void resize(long newHighestTrackableValue)
        {
            int oldNormalizedZeroIndex = normalizeIndex(0, normalizingIndexOffset, countsArrayLength);

            establishSize(newHighestTrackableValue);

            int countsDelta = countsArrayLength - counts.Length;

            Array.Resize(ref counts, countsArrayLength);

            if (oldNormalizedZeroIndex != 0)
            {
                // We need to shift the stuff from the zero index and up to the end of the array:
                int newNormalizedZeroIndex = oldNormalizedZeroIndex + countsDelta;
                int lengthToCopy = (countsArrayLength - countsDelta) - oldNormalizedZeroIndex;
                Array.Copy(counts, oldNormalizedZeroIndex, counts, newNormalizedZeroIndex, lengthToCopy);
            }
        }

        /**
         * Construct an auto-resizing ShortCountsHistogram with a lowest discernible value of 1 and an auto-adjusting
         * highestTrackableValue. Can auto-resize up to track values up to (Long.MAX_VALUE / 2).
         *
         * @param numberOfSignificantValueDigits Specifies the precision to use. This is the number of significant
         *                                       decimal digits to which the histogram will maintain value resolution
         *                                       and separation. Must be a non-negative integer between 0 and 5.
         */
        public ShortCountsHistogram(int numberOfSignificantValueDigits)
            : this(1, 2, numberOfSignificantValueDigits)
        { }

        /**
         * Construct a ShortCountsHistogram given the Highest value to be tracked and a number of significant decimal digits. The
         * histogram will be constructed to implicitly track (distinguish from 0) values as low as 1.
         *
         * @param highestTrackableValue The highest value to be tracked by the histogram. Must be a positive
         *                              integer that is {@literal >=} 2.
         * @param numberOfSignificantValueDigits Specifies the precision to use. This is the number of significant
         *                                       decimal digits to which the histogram will maintain value resolution
         *                                       and separation. Must be a non-negative integer between 0 and 5.
         */
        public ShortCountsHistogram(long highestTrackableValue, int numberOfSignificantValueDigits)
            : this(1, highestTrackableValue, numberOfSignificantValueDigits)
        { }

        /**
         * Construct a ShortCountsHistogram given the Lowest and Highest values to be tracked and a number of significant
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
        public ShortCountsHistogram(long lowestDiscernibleValue, long highestTrackableValue, int numberOfSignificantValueDigits)
            : base(lowestDiscernibleValue, highestTrackableValue, numberOfSignificantValueDigits, sizeof(short), autoResize: true)
        {
            counts = new short[countsArrayLength];
        }

        /**
         * Construct a histogram with the same range settings as a given source histogram,
         * duplicating the source's start/end timestamps (but NOT it's contents)
         * @param source The source histogram to duplicate
         */
        public ShortCountsHistogram(AbstractHistogram source)
            : base(source)
        {
            counts = new short[countsArrayLength];
        }

        /**
         * Construct a new histogram by decoding it from a ByteBuffer.
         * @param buffer The buffer to decode from
         * @param minBarForHighestTrackableValue Force highestTrackableValue to be set at least this high
         * @return The newly constructed histogram
         */
        public static ShortCountsHistogram decodeFromByteBuffer(ByteBuffer buffer, long minBarForHighestTrackableValue)
        {
            return (ShortCountsHistogram)decodeFromByteBuffer(buffer, typeof(ShortCountsHistogram), minBarForHighestTrackableValue);
        }

        /**
         * Construct a new histogram by decoding it from a compressed form in a ByteBuffer.
         * @param buffer The buffer to decode from
         * @param minBarForHighestTrackableValue Force highestTrackableValue to be set at least this high
         * @return The newly constructed histogram
         * @throws DataFormatException on error parsing/decompressing the buffer
         */
        public static ShortCountsHistogram decodeFromCompressedByteBuffer(ByteBuffer buffer, long minBarForHighestTrackableValue)
        {
            return (ShortCountsHistogram)decodeFromCompressedByteBuffer(buffer, typeof(ShortCountsHistogram), minBarForHighestTrackableValue);
        }

        private void readObject(Stream o)
        {
            // TODO: ??
            //o.defaultReadObject();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected internal override void fillCountsArrayFromBuffer(ByteBuffer buffer, int length)
        {
            buffer.asShortBuffer().get(counts, 0, length);
        }

        // We try to cache the LongBuffer used in output cases, as repeated
        // output form the same histogram using the same buffer is likely:
        private ShortBuffer cachedDstShortBuffer = null;
        private ByteBuffer cachedDstByteBuffer = null;
        private int cachedDstByteBufferPosition = 0;

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected internal override void fillBufferFromCountsArray(ByteBuffer buffer, int length)
        {
            if ((cachedDstShortBuffer == null) ||
                    (buffer != cachedDstByteBuffer) ||
                    (buffer.position() != cachedDstByteBufferPosition))
            {
                cachedDstByteBuffer = buffer;
                cachedDstByteBufferPosition = buffer.position();
                cachedDstShortBuffer = buffer.asShortBuffer();
            }
            cachedDstShortBuffer.rewind();
            int zeroIndex = normalizeIndex(0, getNormalizingIndexOffset(), countsArrayLength);
            int lengthFromZeroIndexToEnd = Math.Min(length, (countsArrayLength - zeroIndex));
            int remainingLengthFromNormalizedZeroIndex = length - lengthFromZeroIndexToEnd;
            cachedDstShortBuffer.put(counts, zeroIndex, lengthFromZeroIndexToEnd);
            cachedDstShortBuffer.put(counts, 0, remainingLengthFromNormalizedZeroIndex);
        }
    }
}
