using System;
using System.Linq;
using FluentAssertions;
using Metrics.Sampling;
using Xunit;

namespace Metrics.Tests.Sampling
{
    public class WeightedSnapshotTests
    {
        private readonly WeightedSnapshot snapshot = new WeightedSnapshot(0, Enumerable.Empty<WeightedSample>());

        [Fact]
        public void WeightedSnapshot_ThrowsOnBadQuantileValue()
        {
            ((Action)(() => snapshot.GetValue(-0.5))).ShouldThrow<ArgumentException>();
            ((Action)(() => snapshot.GetValue(1.5))).ShouldThrow<ArgumentException>();
            ((Action)(() => snapshot.GetValue(double.NaN))).ShouldThrow<ArgumentException>();
        }
    }
}
