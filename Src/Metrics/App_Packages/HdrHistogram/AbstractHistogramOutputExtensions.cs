using System;
using System.Globalization;
using System.IO;

namespace HdrHistogram
{
    public static class AbstractHistogramOutputExtensions
    {
        private static readonly CultureInfo usCulture = CultureInfo.CreateSpecificCulture("en-US");

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

        public static void OutputPercentileDistribution(this AbstractHistogram histogram, TextWriter printStream, double outputValueUnitScalingRatio)
        {
            histogram.OutputPercentileDistribution(printStream, 5, outputValueUnitScalingRatio);
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
        public static void OutputPercentileDistribution(this AbstractHistogram histogram, TextWriter printStream,
                                                 int percentileTicksPerHalfDistance,
                                                 double outputValueUnitScalingRatio)
        {
            histogram.OutputPercentileDistribution(printStream, percentileTicksPerHalfDistance, outputValueUnitScalingRatio, false);
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
        public static void OutputPercentileDistribution(this AbstractHistogram histogram, TextWriter printStream,
                                                 int percentileTicksPerHalfDistance,
                                                 double outputValueUnitScalingRatio,
                                                 bool useCsvFormat)
        {

            // TODO: fix format strings 

            if (useCsvFormat)
            {
                printStream.Write("\"Value\",\"Percentile\",\"TotalCount\",\"1/(1-Percentile)\"\n");
            }
            else
            {
                printStream.Write("{0,12} {1,14} {2,10} {3,14}\n\n", "Value", "Percentile", "TotalCount", "1/(1-Percentile)");
            }

            String percentileFormatString;
            String lastLinePercentileFormatString;
            if (useCsvFormat)
            {
                percentileFormatString = "%." + histogram.NumberOfSignificantValueDigits + "f,%.12f,%d,%.2f\n";
                lastLinePercentileFormatString = "%." + histogram.NumberOfSignificantValueDigits + "f,%.12f,%d,Infinity\n";
            }
            else
            {
                percentileFormatString = "%12." + histogram.NumberOfSignificantValueDigits + "f %2.12f %10d %14.2f\n";
                lastLinePercentileFormatString = "%12." + histogram.NumberOfSignificantValueDigits + "f %2.12f %10d\n";
            }

            foreach (var iterationValue in histogram.Percentiles(percentileTicksPerHalfDistance))
            {
                if (iterationValue.getPercentileLevelIteratedTo() != 100.0D)
                {
                    printStream.Write(string.Format(usCulture, percentileFormatString,
                        iterationValue.getValueIteratedTo() / outputValueUnitScalingRatio,
                        iterationValue.getPercentileLevelIteratedTo() / 100.0D,
                        iterationValue.getTotalCountToThisValue(),
                        1 / (1.0D - (iterationValue.getPercentileLevelIteratedTo() / 100.0D))));
                }
                else
                {
                    printStream.Write(string.Format(usCulture, lastLinePercentileFormatString,
                        iterationValue.getValueIteratedTo() / outputValueUnitScalingRatio,
                        iterationValue.getPercentileLevelIteratedTo() / 100.0D,
                        iterationValue.getTotalCountToThisValue()));
                }
            }

            if (!useCsvFormat)
            {
                // Calculate and output mean and std. deviation.
                // Note: mean/std. deviation numbers are very often completely irrelevant when
                // data is extremely non-normal in distribution (e.g. in cases of strong multi-modal
                // response time distribution associated with GC pauses). However, reporting these numbers
                // can be very useful for contrasting with the detailed percentile distribution
                // reported by outputPercentileDistribution(). It is not at all surprising to find
                // percentile distributions where results fall many tens or even hundreds of standard
                // deviations away from the mean - such results simply indicate that the data sampled
                // exhibits a very non-normal distribution, highlighting situations for which the std.
                // deviation metric is a useless indicator.
                //

                double mean = histogram.getMean() / outputValueUnitScalingRatio;
                double std_deviation = histogram.getStdDeviation() / outputValueUnitScalingRatio;
                printStream.Write(string.Format(usCulture,
                        "#[Mean    = %12." + histogram.NumberOfSignificantValueDigits + "f, StdDeviation   = %12." +
                                histogram.NumberOfSignificantValueDigits + "f]\n",
                        mean, std_deviation));
                printStream.Write(string.Format(usCulture,
                        "#[Max     = %12." + histogram.NumberOfSignificantValueDigits + "f, Total count    = %12d]\n",
                        histogram.getMaxValue() / outputValueUnitScalingRatio, histogram.getTotalCount()));
                printStream.Write(string.Format(usCulture, "#[Buckets = %12d, SubBuckets     = %12d]\n",
                        histogram.bucketCount, histogram.subBucketCount));
            }
        }
    }
}
