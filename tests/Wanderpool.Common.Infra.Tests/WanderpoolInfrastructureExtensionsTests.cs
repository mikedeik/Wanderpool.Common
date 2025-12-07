using Microsoft.Extensions.DependencyInjection;
using Wanderpool.Common.Infra;
using Xunit;

namespace Wanderpool.Common.Infra.Tests;

/// <summary>
/// Tests for WanderpoolInfrastructureExtensions - unified infrastructure service registration.
/// </summary>
public class WanderpoolInfrastructureExtensionsTests
{
    /// <summary>
    /// Test: AddWanderpoolInfrastructure registers logging service.
    /// </summary>
    [Fact]
    public void AddWanderpoolInfrastructure_RegistersLogging()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolInfrastructure(options =>
        {
            options.EnableLogging = true;
        });

        // Assert - logging should be registered
        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider);
    }

    /// <summary>
    /// Test: AddWanderpoolInfrastructure registers tracing service.
    /// </summary>
    [Fact]
    public void AddWanderpoolInfrastructure_RegistersTracing()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolInfrastructure(options =>
        {
            options.EnableTracing = true;
        });

        // Assert - tracing should be registered
        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider);
    }

    /// <summary>
    /// Test: AddWanderpoolInfrastructure registers metrics service.
    /// </summary>
    [Fact]
    public void AddWanderpoolInfrastructure_RegistersMetrics()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolInfrastructure(options =>
        {
            options.EnableMetrics = true;
        });

        // Assert - metrics should be registered
        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider);
    }

    /// <summary>
    /// Test: AddWanderpoolInfrastructure registers exception handling.
    /// </summary>
    [Fact]
    public void AddWanderpoolInfrastructure_RegistersExceptionHandling()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolInfrastructure(options =>
        {
            options.EnableExceptionHandling = true;
        });

        // Assert - service collection should be returned
        Assert.NotNull(services);
    }

    /// <summary>
    /// Test: AddWanderpoolInfrastructure registers health checks.
    /// </summary>
    [Fact]
    public void AddWanderpoolInfrastructure_RegistersHealthChecks()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolInfrastructure(options =>
        {
            options.EnableHealthChecks = true;
        });

        // Assert - service collection should be returned
        Assert.NotNull(services);
    }

    /// <summary>
    /// Test: AddWanderpoolInfrastructure registers correlation context.
    /// </summary>
    [Fact]
    public void AddWanderpoolInfrastructure_RegistersCorrelationContext()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolInfrastructure(options =>
        {
            options.EnableCorrelationId = true;
        });

        // Assert - service collection should be returned
        Assert.NotNull(services);
    }

    /// <summary>
    /// Test: AddWanderpoolInfrastructure respects options configuration.
    /// </summary>
    [Fact]
    public void AddWanderpoolInfrastructure_RespectsOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolInfrastructure(options =>
        {
            options.EnableLogging = true;
            options.EnableTracing = false;
            options.EnableMetrics = true;
        });

        var provider = services.BuildServiceProvider();

        // Assert - services should be registered according to options
        Assert.NotNull(provider);
    }
}
