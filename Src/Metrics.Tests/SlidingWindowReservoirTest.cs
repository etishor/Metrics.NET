using FluentAssertions;
using Metrics.Sampling;
using Xunit;

namespace Metrics.Tests
{
    public class SlidingWindowReservoirTest
    {
        private readonly SlidingWindowReservoir reservoir = new SlidingWindowReservoir(3);

        [Fact]
        public void SlidingWindowReservoirCanStoreSmallSample()
        {
            reservoir.Update(1L);
            reservoir.Update(2L);

            reservoir.Snapshot.Values.Should().ContainInOrder(1L, 2L);
        }

        [Fact]
        public void SlidingWindowReservoirOnlyStoresLastsValues()
        {
            reservoir.Update(1L);
            reservoir.Update(2L);
            reservoir.Update(3L);
            reservoir.Update(4L);
            reservoir.Update(5L);

            reservoir.Snapshot.Values.Should().ContainInOrder(3L, 4L, 5L);
        }
    }
}
