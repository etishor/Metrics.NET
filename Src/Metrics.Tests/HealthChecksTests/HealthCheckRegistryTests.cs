using System;
using FluentAssertions;
using Metrics.Core;
using Xunit;

namespace Metrics.Tests.HealthChecksTests
{
    public class HealthCheckRegistryTests
    {
        [Fact]
        public void HealthCheck_RegistryExecutesCheckOnEachGetStatus()
        {
            HealthCheckRegistry registry = new HealthCheckRegistry();
            int count = 0;

            registry.RegisterHealthCheck(new HealthCheck("test", () => { count++; }));

            count.Should().Be(0);

            registry.GetStatus();

            count.Should().Be(1);

            registry.GetStatus();

            count.Should().Be(2);
        }

        [Fact]
        public void HealthCheck_RegistryStatusIsFailedIfOneCheckFails()
        {
            HealthCheckRegistry registry = new HealthCheckRegistry();

            registry.RegisterHealthCheck(new HealthCheck("ok", () => { }));
            registry.RegisterHealthCheck(new HealthCheck("bad", () => HealthCheckResult.Unhealthy()));

            var status = registry.GetStatus();

            status.IsHealthy.Should().BeFalse();
            status.Results.Length.Should().Be(2);
        }

        [Fact]
        public void HealthCheck_RegistryStatusIsHealthyIfAllChecksAreHealthy()
        {
            HealthCheckRegistry registry = new HealthCheckRegistry();

            registry.RegisterHealthCheck(new HealthCheck("ok", () => { }));
            registry.RegisterHealthCheck(new HealthCheck("another", () => HealthCheckResult.Healthy()));

            var status = registry.GetStatus();

            status.IsHealthy.Should().BeTrue();
            status.Results.Length.Should().Be(2);
        }

        [Fact]
        public void HealthCheck_RegistryDoesNotThrowOnDuplicateRegistration()
        {
            HealthCheckRegistry registry = new HealthCheckRegistry();

            registry.RegisterHealthCheck(new HealthCheck("test", () => { }));

            Action action = () => registry.RegisterHealthCheck(new HealthCheck("test", () => { }));
            action.ShouldNotThrow<InvalidOperationException>();
        }
    }
}
