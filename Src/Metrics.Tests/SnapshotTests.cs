using FluentAssertions;
using Metrics.Sampling;
using Xunit;

namespace Metrics.Tests
{
    public class UniformSnapshotTest
    {
        private readonly UniformSnapshot snapshot = new UniformSnapshot(new long[] { 5, 1, 2, 3, 4 });

        [Fact]
        public void SnapshotSmallQuantilesAreTheFirstValue()
        {
            snapshot.GetValue(0.0).Should().BeApproximately(1, 0.1);
        }

        [Fact]
        public void SnapshotBigQuantilesAreTheLastValue()
        {
            snapshot.GetValue(1.0).Should().BeApproximately(5, 0.1);
        }

        [Fact]
        public void SnapshotHasAMedian()
        {
            snapshot.Median.Should().BeApproximately(3, 0.1);
        }

        [Fact]
        public void SnapshotHasAp75()
        {
            snapshot.Percentile75.Should().BeApproximately(4.5, 0.1);
        }

        [Fact]
        public void SnapshotHasAp95()
        {
            snapshot.Percentile95.Should().BeApproximately(5.0, 0.1);
        }

        [Fact]
        public void SnapshotHasAp98()
        {
            snapshot.Percentile98.Should().BeApproximately(5.0, 0.1);
        }

        [Fact]
        public void SnapshotHasAp99()
        {
            snapshot.Percentile99.Should().BeApproximately(5.0, 0.1);
        }

        [Fact]
        public void SnapshotHasAp999()
        {
            snapshot.Percentile999.Should().BeApproximately(5.0, 0.1);
        }

        [Fact]
        public void SnapshotHasValues()
        {
            snapshot.Values.Should().ContainInOrder(1L, 2L, 3L, 4L, 5L);
        }

        [Fact]
        public void SnapshotHasASize()
        {
            snapshot.Size.Should().Be(5);
        }

        [Fact]
        public void SnapshotCalculatesTheMinimumValue()
        {
            snapshot.Min.Should().Be(1);
        }

        [Fact]
        public void SnapshotCalculatesTheMaximumValue()
        {
            snapshot.Max.Should().Be(5);
        }

        [Fact]
        public void SnapshotCalculatesTheMeanValue()
        {
            snapshot.Mean.Should().Be(3.0);
        }

        [Fact]
        public void SnapshotCalculatesTheStdDev()
        {
            snapshot.StdDev.Should().BeApproximately(1.5811, 0.0001);
        }

        [Fact]
        public void SnapshotCalculatesAMinOfZeroForAnEmptySnapshot()
        {
            Snapshot snapshot = new UniformSnapshot(new long[] { });
            snapshot.Min.Should().Be(0);
        }

        [Fact]
        public void SnapshotCalculatesAMaxOfZeroForAnEmptySnapshot()
        {
            Snapshot snapshot = new UniformSnapshot(new long[] { });
            snapshot.Max.Should().Be(0);
        }

        [Fact]
        public void SnapshotCalculatesAMeanOfZeroForAnEmptySnapshot()
        {
            Snapshot snapshot = new UniformSnapshot(new long[] { });
            snapshot.Mean.Should().Be(0);
        }

        [Fact]
        public void SnapshotCalculatesAStdDevOfZeroForAnEmptySnapshot()
        {
            Snapshot snapshot = new UniformSnapshot(new long[] { });
            snapshot.StdDev.Should().Be(0);
        }

        [Fact]
        public void SnapshotCalculatesAStdDevOfZeroForASingletonSnapshot()
        {
            Snapshot snapshot = new UniformSnapshot(new long[] { 1 });
            snapshot.StdDev.Should().Be(0);
        }
    }
}
