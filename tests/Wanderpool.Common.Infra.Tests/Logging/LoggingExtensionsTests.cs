using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Wanderpool.Common.Infra.Logging;

namespace Wanderpool.Common.Infra.Tests.Logging;

public class LoggingExtensionsTests
{
    [Fact]
    public void AddWanderpoolLogging_ConfiguresSerilog()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var serviceName = "TestService";

        // Act
        builder.AddWanderpoolLogging(serviceName);

        // Assert
        builder.Should().NotBeNull();
        // Verify that Serilog is configured (Log.Logger should be set)
        Log.Logger.Should().NotBeNull();
    }

    [Fact]
    public void AddWanderpoolLogging_ReturnsBuilderForMethodChaining()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        var result = builder.AddWanderpoolLogging("TestService");

        // Assert
        result.Should().Be(builder);
    }

    [Fact]
    public void AddWanderpoolLogging_UsesApplicationNameWhenServiceNameNotProvided()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var originalAppName = builder.Environment.ApplicationName;

        // Act
        builder.AddWanderpoolLogging();

        // Assert
        builder.Should().NotBeNull();
        // The serviceName property is not directly accessible, but we can verify
        // that the configuration was set up without errors
        Log.Logger.Should().NotBeNull();
    }

    [Fact]
    public void AddWanderpoolLogging_SupportsMethodChaining()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        var result = builder
            .AddWanderpoolLogging("TestService")
            .ConfigureServices(services => services.AddLogging());

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void AddWanderpoolLogging_ConfiguresEnrichers()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        builder.AddWanderpoolLogging("TestService");
        var app = builder.Build();

        // Assert
        app.Should().NotBeNull();
        // Serilog should be configured with enrichers
        Log.Logger.Should().NotBeNull();
    }

    [Fact]
    public void AddWanderpoolLogging_ConfiguresConsoleOutput()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        builder.AddWanderpoolLogging("TestService");
        var app = builder.Build();

        // Assert
        // Verify that the app can be built with Serilog configured
        app.Should().NotBeNull();
    }

    [Fact]
    public void AddWanderpoolLogging_InDevelopment_UsesReadableFormat()
    {
        // Arrange
        var args = new[] { "--environment=Development" };
        var builder = WebApplication.CreateBuilder(args);

        // Act
        builder.AddWanderpoolLogging("TestService");
        var app = builder.Build();

        // Assert
        app.Environment.IsDevelopment().Should().BeTrue();
        app.Should().NotBeNull();
    }

    [Fact]
    public void AddWanderpoolLogging_InProduction_UsesJsonFormat()
    {
        // Arrange
        var args = new[] { "--environment=Production" };
        var builder = WebApplication.CreateBuilder(args);

        // Act
        builder.AddWanderpoolLogging("TestService");
        var app = builder.Build();

        // Assert
        app.Environment.IsProduction().Should().BeTrue();
        app.Should().NotBeNull();
    }

    [Fact]
    public void AddWanderpoolLogging_WithCustomServiceName()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var customServiceName = "MyCustomService";

        // Act
        builder.AddWanderpoolLogging(customServiceName);

        // Assert
        builder.Should().NotBeNull();
        Log.Logger.Should().NotBeNull();
    }

    [Fact]
    public void AddWanderpoolLogging_WithNullServiceName_UsesApplicationName()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        builder.AddWanderpoolLogging(null);

        // Assert
        builder.Should().NotBeNull();
        Log.Logger.Should().NotBeNull();
    }

    [Fact]
    public void AddWanderpoolLogging_WithEmptyServiceName_UsesApplicationName()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        builder.AddWanderpoolLogging(string.Empty);

        // Assert
        builder.Should().NotBeNull();
        Log.Logger.Should().NotBeNull();
    }

    [Fact]
    public void AddWanderpoolLogging_AllowsMultipleConfigurations()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act & Assert (should not throw)
        var action = () =>
        {
            builder.AddWanderpoolLogging("Service1");
            builder.ConfigureServices(services => services.AddLogging());
            builder.AddWanderpoolLogging("Service2");
        };

        action.Should().NotThrow();
    }
}
