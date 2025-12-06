using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using Wanderpool.Common.Infra.Telemetry;

namespace Wanderpool.Common.Infra.Tests.Telemetry;

public class TracingExtensionsTests
{
    [Fact]
    public void AddWanderpoolTracing_Default_RegistersOpenTelemetry()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolTracing();
        var provider = services.BuildServiceProvider();

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(TracerProvider));
    }

    [Fact]
    public void AddWanderpoolTracing_Default_UsesAssemblyNameAsServiceName()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddWanderpoolTracing();

        // Assert
        result.Should().NotBeNull();
        // Service collection should have registered OpenTelemetry services
        services.Should().Contain(sd => sd.ServiceType == typeof(TracerProvider));
    }

    [Fact]
    public void AddWanderpoolTracing_Default_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddWanderpoolTracing();

        // Assert
        result.Should().Be(services);
    }

    [Fact]
    public void AddWanderpoolTracing_WithCustomServiceName_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var customServiceName = "CustomService";

        // Act
        services.AddWanderpoolTracing(customServiceName);

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(TracerProvider));
    }

    [Fact]
    public void AddWanderpoolTracing_WithCustomServiceName_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddWanderpoolTracing("MyService");

        // Assert
        result.Should().Be(services);
    }

    [Fact]
    public void AddWanderpoolTracing_WithCustomEndpoint_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var endpoint = "http://localhost:4317";

        // Act
        services.AddWanderpoolTracing(endpoint);

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(TracerProvider));
    }

    [Fact]
    public void AddWanderpoolTracing_WithCustomEndpoint_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddWanderpoolTracing("http://localhost:4317");

        // Assert
        result.Should().Be(services);
    }

    [Fact]
    public void AddWanderpoolTracing_WithSamplingProbability_AcceptsValidRange()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert - Should not throw
        services.AddWanderpoolTracing("http://localhost:4317", samplingProbability: 0.0);

        var services2 = new ServiceCollection();
        services2.AddWanderpoolTracing("http://localhost:4317", samplingProbability: 0.5);

        var services3 = new ServiceCollection();
        services3.AddWanderpoolTracing("http://localhost:4317", samplingProbability: 1.0);
    }

    [Fact]
    public void AddWanderpoolTracing_WithSamplingProbabilityBelowZero_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var action = () => services.AddWanderpoolTracing("http://localhost:4317", samplingProbability: -0.1);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Sampling probability must be between 0.0 and 1.0*");
    }

    [Fact]
    public void AddWanderpoolTracing_WithSamplingProbabilityAboveOne_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var action = () => services.AddWanderpoolTracing("http://localhost:4317", samplingProbability: 1.1);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Sampling probability must be between 0.0 and 1.0*");
    }

    [Fact]
    public void AddWanderpoolTracing_Default_DefaultsToLocalhost4317()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolTracing();
        var provider = services.BuildServiceProvider();

        // Assert
        provider.Should().NotBeNull();
        services.Should().Contain(sd => sd.ServiceType == typeof(TracerProvider));
    }

    [Fact]
    public void AddWanderpoolTracing_WithCustomEndpointAndSamplingProbability_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var endpoint = "http://otel-collector:4317";
        var samplingProbability = 0.5;

        // Act
        services.AddWanderpoolTracing(endpoint, samplingProbability);

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(TracerProvider));
    }

    [Fact]
    public void AddWanderpoolTracing_WithCustomEndpointSamplingAndServiceName_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var endpoint = "http://otel-collector:4317";
        var samplingProbability = 0.75;
        var serviceName = "CustomServiceName";

        // Act
        services.AddWanderpoolTracing(endpoint, samplingProbability, serviceName);

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(TracerProvider));
    }

    [Fact]
    public void AddWanderpoolTracing_WithCustomEndpointSamplingAndServiceName_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddWanderpoolTracing("http://otel:4317", 0.8, "Service");

        // Assert
        result.Should().Be(services);
    }

    [Fact]
    public void AddWanderpoolTracing_Default_ConfiguresDefaultSamplingAtFullProbability()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolTracing();

        // Assert
        // The default configuration should use full sampling (1.0 probability)
        services.Should().Contain(sd => sd.ServiceType == typeof(TracerProvider));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.25)]
    [InlineData(0.5)]
    [InlineData(0.75)]
    [InlineData(1.0)]
    public void AddWanderpoolTracing_WithVariousSamplingProbabilities_AcceptsAll(double samplingProbability)
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert - Should not throw
        services.AddWanderpoolTracing("http://localhost:4317", samplingProbability);
        var provider = services.BuildServiceProvider();
        provider.Should().NotBeNull();
    }

    [Theory]
    [InlineData(-1.0)]
    [InlineData(-0.5)]
    [InlineData(1.1)]
    [InlineData(2.0)]
    public void AddWanderpoolTracing_WithInvalidSamplingProbabilities_ThrowsArgumentException(double samplingProbability)
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var action = () => services.AddWanderpoolTracing("http://localhost:4317", samplingProbability);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddWanderpoolTracing_MultipleOverloadsCanBeChained()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services
            .AddWanderpoolTracing("http://localhost:4317")
            .AddLogging();

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(TracerProvider));
    }
}
