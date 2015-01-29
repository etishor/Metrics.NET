using System.Linq;
using FluentAssertions;
using Metrics.Sampling;
using Xunit;

namespace Metrics.Tests.Sampling
{
    public class SlidingWindowReservoirTest
    {
        private readonly SlidingWindowReservoir reservoir = new SlidingWindowReservoir(3);

        [Fact]
        public void SlidingWindowReservoir_CanStoreSmallSample()
        {
            reservoir.Update(1L);
            reservoir.Update(2L);

            reservoir.GetSnapshot().Values.Select(val => val.Item1).Should().ContainInOrder(1L, 2L);
        }

        [Fact]
        public void SlidingWindowReservoir_OnlyStoresLastsValues()
        {
            reservoir.Update(1L);
            reservoir.Update(2L);
            reservoir.Update(3L);
            reservoir.Update(4L);
            reservoir.Update(5L);

            reservoir.GetSnapshot().Values.Select(val => val.Item1).Should().ContainInOrder(3L, 4L, 5L);
        }

        [Fact]
        public void SlidingWindowReservoir_RecordsUserValue()
        {
            reservoir.Update(2L, "B");
            reservoir.Update(1L, "A");

            reservoir.GetSnapshot().MinUserValue.Should().Be("A");
            reservoir.GetSnapshot().MaxUserValue.Should().Be("B");
        }

        [Fact]
        public void SlidingWindowReservoir_MergeIsApplied()
        {
            var other = new SlidingWindowReservoir();

            reservoir.Update(1L);
            reservoir.Update(2L);
            reservoir.Update(3L);
            
            other.Update(4L);
            other.Update(5L);

            reservoir.Merge(other);

            reservoir.GetSnapshot().Values.Select(val => val.Item1).Should().ContainInOrder(3L, 4L, 5L);
        }

        [Fact]
        public void SlidingWindowReservoir_MergeCarriesUserValues()
        {
            var other = new SlidingWindowReservoir();

            reservoir.Update(2L, "B");
            reservoir.Update(3L, "C");

            other.Update(1L, "A");
            other.Update(4L, "D");

            reservoir.Merge(other);

            reservoir.GetSnapshot().MinUserValue.Should().Be("A");
            reservoir.GetSnapshot().MaxUserValue.Should().Be("D");
        }
    }
}
