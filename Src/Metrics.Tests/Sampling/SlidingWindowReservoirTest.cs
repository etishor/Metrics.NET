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

            reservoir.Snapshot.Values.Should().ContainInOrder(1L, 2L);
        }

        [Fact]
        public void SlidingWindowReservoir_OnlyStoresLastsValues()
        {
            reservoir.Update(1L);
            reservoir.Update(2L);
            reservoir.Update(3L);
            reservoir.Update(4L);
            reservoir.Update(5L);

            reservoir.Snapshot.Values.Should().ContainInOrder(3L, 4L, 5L);
        }

        [Fact]
        public void SlidingWindowReservoir_RecordsUserValue()
        {
            reservoir.Update(2L, "B");
            reservoir.Update(1L, "A");

            reservoir.Snapshot.MinUserValue.Should().Be("A");
            reservoir.Snapshot.MaxUserValue.Should().Be("B");
        }
    }
}
