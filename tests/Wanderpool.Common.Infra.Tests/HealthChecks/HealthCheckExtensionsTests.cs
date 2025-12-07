using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Wanderpool.Common.Infra.HealthChecks;
using Xunit;

namespace Wanderpool.Common.Infra.Tests.HealthChecks;

/// <summary>
/// Tests for HealthCheckExtensions - service extension for health check registration.
/// </summary>
public class HealthCheckExtensionsTests
{
    /// <summary>
    /// Test: AddWanderpoolHealthChecks returns service collection for chaining.
    /// </summary>
    [Fact]
    public void AddWanderpoolHealthChecks_WithoutConfig_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddWanderpoolHealthChecks();

        // Assert
        Assert.Same(services, result);
    }

    /// <summary>
    /// Test: AddWanderpoolHealthChecks with config returns service collection.
    /// </summary>
    [Fact]
    public void AddWanderpoolHealthChecks_WithConfig_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddWanderpoolHealthChecks(builder => { });

        // Assert
        Assert.Same(services, result);
    }

    /// <summary>
    /// Test: HealthCheckService is registered after AddWanderpoolHealthChecks.
    /// </summary>
    [Fact]
    public void AddWanderpoolHealthChecks_RegistersHealthCheckService()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolHealthChecks();
        var provider = services.BuildServiceProvider();

        // Assert - HealthCheckService should be registered
        var healthCheckService = provider.GetService<HealthCheckService>();
        Assert.NotNull(healthCheckService);
    }

    /// <summary>
    /// Test: Extension method returns service collection for chaining.
    /// </summary>
    [Fact]
    public void AddWanderpoolHealthChecks_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddWanderpoolHealthChecks();

        // Assert - should return the service collection for chaining
        Assert.NotNull(result);
        Assert.Same(services, result);
    }

    /// <summary>
    /// Test: Multiple calls to AddWanderpoolHealthChecks don't cause issues.
    /// </summary>
    [Fact]
    public void AddWanderpoolHealthChecks_CanBeCalledMultipleTimes()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolHealthChecks();
        services.AddWanderpoolHealthChecks();

        // Assert - should not throw
        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider);
    }
}
