using FluentAssertions;
using Metrics.Sampling;
using Xunit;

namespace Metrics.Tests
{
    public class UniformReservoirTests
    {
        [Fact]
        public void UniformReservoirOf100OutOf1000Elements()
        {
            UniformReservoir reservoir = new UniformReservoir(100);

            for (int i = 0; i < 1000; i++)
            {
                reservoir.Update(i);
            }

            reservoir.Size.Should().Be(100);
            reservoir.Snapshot.Size.Should().Be(100);
            reservoir.Snapshot.Values.Should().OnlyContain(v => 0 <= v && v < 1000);
        }
    }
}
