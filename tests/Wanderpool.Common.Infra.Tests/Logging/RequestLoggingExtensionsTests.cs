using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using Wanderpool.Common.Infra.Logging;

namespace Wanderpool.Common.Infra.Tests.Logging;

/// <summary>
/// Tests for RequestLoggingExtensions fluent API.
/// </summary>
public class RequestLoggingExtensionsTests
{
    /// <summary>
    /// Test: UseWanderpoolRequestLogging registers middleware and returns app for chaining.
    /// </summary>
    [Fact]
    public void UseWanderpoolRequestLogging_RegistersMiddleware()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddOptions<RequestLoggingOptions>();

        var app = builder.Build();

        // Act
        var result = app.UseWanderpoolRequestLogging();

        // Assert - verify it returns the app for chaining
        Assert.NotNull(result);
        Assert.Same(app, result);
    }

    /// <summary>
    /// Test: Extension method accepts optional configuration parameter.
    /// </summary>
    [Fact]
    public void UseWanderpoolRequestLogging_AcceptsConfigurationParameter()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddOptions<RequestLoggingOptions>();

        var app = builder.Build();

        // Act - call with configuration
        var result = app.UseWanderpoolRequestLogging(options =>
        {
            options.EnableResponseBodyLogging = true;
            options.MaxBodySizeLogged = 8192;
        });

        // Assert
        Assert.NotNull(result);
        Assert.Same(app, result);
    }

    /// <summary>
    /// Test: Extension is chainable with other middleware registration.
    /// </summary>
    [Fact]
    public void UseWanderpoolRequestLogging_IsChainable()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddOptions<RequestLoggingOptions>();

        var app = builder.Build();

        // Act - chain multiple middleware registrations
        var result = app
            .UseWanderpoolRequestLogging()
            .UseRouting();

        // Assert
        Assert.NotNull(result);
        Assert.Same(app, result);
    }

    /// <summary>
    /// Test: Null application throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void UseWanderpoolRequestLogging_WithNullApp_ThrowsArgumentNullException()
    {
        // Arrange
        WebApplication? nullApp = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            LoggingExtensions.UseWanderpoolRequestLogging(nullApp!));
    }
}
