using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Wanderpool.Common.Infra.Telemetry;

namespace Wanderpool.Common.Infra.Tests.Telemetry;

/// <summary>
/// Tests for Prometheus metrics endpoint configuration.
/// </summary>
public class PrometheusEndpointTests
{
    [Fact]
    public void MapWanderpoolMetrics_ReturnsEndpointConventionBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddWanderpoolMetrics();
        var app = WebApplication.CreateBuilder().Build();

        // Act
        var result = app.MapWanderpoolMetrics();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void MapWanderpoolMetrics_WithCustomPath_ReturnsEndpointConventionBuilder()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddWanderpoolMetrics();
        var app = WebApplication.CreateBuilder().Build();

        // Act
        var result = app.MapWanderpoolMetrics("/prometheus");

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void AddWanderpoolMetrics_RegistersServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolMetrics();
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider);
        // Verify the service collection was registered
        Assert.NotEmpty(services);
    }

    [Fact]
    public void MapWanderpoolMetrics_WithNullApp_ThrowsArgumentNullException()
    {
        // Arrange
        WebApplication? app = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => app!.MapWanderpoolMetrics());
    }

    [Fact]
    public void MapWanderpoolMetrics_WithEmptyPath_ThrowsArgumentException()
    {
        // Arrange
        var app = WebApplication.CreateBuilder().Build();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => app.MapWanderpoolMetrics(""));
    }

    [Fact]
    public void MapWanderpoolMetrics_WithWhitespacePath_ThrowsArgumentException()
    {
        // Arrange
        var app = WebApplication.CreateBuilder().Build();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => app.MapWanderpoolMetrics("   "));
    }
}
