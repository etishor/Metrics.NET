using System;
using FluentAssertions;
using Metrics.Core;
using Xunit;

namespace Metrics.Tests
{
    public class HealthCheckRegistryTests
    {
        [Fact]
        public void HealthCheckRegistryExecutesCheckOnEachGetStatus()
        {
            int count = 0;

            var registry = new LocalHealthChecksRegistry();
            registry.Register(new HealthCheck("test", () => { count++; }));

            count.Should().Be(0);

            registry.GetStatus();

            count.Should().Be(1);

            registry.GetStatus();

            count.Should().Be(2);
        }

        [Fact]
        public void HealthCheckRegistryStatusIsFailedIfOneCheckFails()
        {
            var registry = new LocalHealthChecksRegistry();
            registry.Register(new HealthCheck("ok", () => { }));
            registry.Register(new HealthCheck("bad", () => HealthCheckResult.Unhealthy()));

            var status = registry.GetStatus();

            status.IsHealty.Should().BeFalse();
            status.Results.Length.Should().Be(2);
        }

        [Fact]
        public void HealthCheckRegistryStatusIsHealthyIfAllChecksAreHealthy()
        {
            var registry = new LocalHealthChecksRegistry();
            registry.Register(new HealthCheck("ok", () => { }));
            registry.Register(new HealthCheck("another", () => HealthCheckResult.Healthy()));

            var status = registry.GetStatus();

            status.IsHealty.Should().BeTrue();
            status.Results.Length.Should().Be(2);
        }

        [Fact]
        public void HealthCheckRegistryThrowsOnDuplicateRegistration()
        {
            var registry = new LocalHealthChecksRegistry();
            registry.Register(new HealthCheck("test", () => { }));

            Action action = () => registry.Register(new HealthCheck("test", () => { }));
            action.ShouldThrow<InvalidOperationException>();
        }
    }
}
