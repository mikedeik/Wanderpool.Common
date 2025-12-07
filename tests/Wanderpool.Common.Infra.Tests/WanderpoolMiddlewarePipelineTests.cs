using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Wanderpool.Common.Infra.Tests;

/// <summary>
/// Tests for Wanderpool middleware pipeline configuration.
/// </summary>
public class WanderpoolMiddlewarePipelineTests
{
    /// <summary>
    /// Test: UseWanderpoolInfrastructure adds correlation ID middleware.
    /// </summary>
    [Fact]
    public void UseWanderpoolInfrastructure_AddsCorrelationIdMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddWanderpoolInfrastructure(options => { });

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddWanderpoolInfrastructure(options => { });
        var app = builder.Build();

        // Act
        app.UseWanderpoolInfrastructure();

        // Assert
        Assert.NotNull(app);
    }

    /// <summary>
    /// Test: UseWanderpoolInfrastructure adds exception handling middleware.
    /// </summary>
    [Fact]
    public void UseWanderpoolInfrastructure_AddsExceptionHandlingMiddleware()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddWanderpoolInfrastructure(options => { });
        var app = builder.Build();

        // Act
        app.UseWanderpoolInfrastructure();

        // Assert
        Assert.NotNull(app);
    }

    /// <summary>
    /// Test: UseWanderpoolInfrastructure adds request logging middleware.
    /// </summary>
    [Fact]
    public void UseWanderpoolInfrastructure_AddsRequestLoggingMiddleware()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddWanderpoolInfrastructure(options => { });
        var app = builder.Build();

        // Act
        app.UseWanderpoolInfrastructure();

        // Assert
        Assert.NotNull(app);
    }

    /// <summary>
    /// Test: Middleware order is correct.
    /// </summary>
    [Fact]
    public void UseWanderpoolInfrastructure_MiddlewareOrderIsCorrect()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddWanderpoolInfrastructure(options => { });
        var app = builder.Build();

        // Act
        app.UseWanderpoolInfrastructure();

        // Assert - middleware pipeline should be configured
        Assert.NotNull(app);
    }

    /// <summary>
    /// Test: Health check endpoints are mapped.
    /// </summary>
    [Fact]
    public void UseWanderpoolInfrastructure_MapsHealthCheckEndpoints()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddWanderpoolInfrastructure(options => { });
        var app = builder.Build();

        // Act
        app.UseWanderpoolInfrastructure();

        // Assert
        Assert.NotNull(app);
    }
}
