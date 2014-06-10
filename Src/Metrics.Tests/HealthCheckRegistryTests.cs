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
            HealthChecks.UnregisterAllHealthChecks();
            int count = 0;

            HealthChecks.RegisterHealthCheck(new HealthCheck("test", () => { count++; }));

            count.Should().Be(0);

            HealthChecks.GetStatus();

            count.Should().Be(1);

            HealthChecks.GetStatus();

            count.Should().Be(2);
        }

        [Fact]
        public void HealthCheckRegistryStatusIsFailedIfOneCheckFails()
        {
            HealthChecks.UnregisterAllHealthChecks();

            HealthChecks.RegisterHealthCheck(new HealthCheck("ok", () => { }));
            HealthChecks.RegisterHealthCheck(new HealthCheck("bad", () => HealthCheckResult.Unhealthy()));

            var status = HealthChecks.GetStatus();

            status.IsHealty.Should().BeFalse();
            status.Results.Length.Should().Be(2);
        }

        [Fact]
        public void HealthCheckRegistryStatusIsHealthyIfAllChecksAreHealthy()
        {
            HealthChecks.UnregisterAllHealthChecks();

            HealthChecks.RegisterHealthCheck(new HealthCheck("ok", () => { }));
            HealthChecks.RegisterHealthCheck(new HealthCheck("another", () => HealthCheckResult.Healthy()));

            var status = HealthChecks.GetStatus();

            status.IsHealty.Should().BeTrue();
            status.Results.Length.Should().Be(2);
        }

        [Fact]
        public void HealthCheckRegistryDoesNotThrowOnDuplicateRegistration()
        {
            HealthChecks.UnregisterAllHealthChecks();

            HealthChecks.RegisterHealthCheck(new HealthCheck("test", () => { }));

            Action action = () => HealthChecks.RegisterHealthCheck(new HealthCheck("test", () => { }));
            action.ShouldNotThrow<InvalidOperationException>();
        }
    }
}
