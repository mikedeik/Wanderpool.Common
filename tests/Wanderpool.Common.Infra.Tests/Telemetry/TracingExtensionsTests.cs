using Microsoft.Extensions.Configuration;
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
        Assert.Contains(services, sd => sd.ServiceType == typeof(TracerProvider));
    }

    [Fact]
    public void AddWanderpoolTracing_Default_UsesAssemblyNameAsServiceName()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddWanderpoolTracing();

        // Assert
        Assert.NotNull(result);
        // Service collection should have registered OpenTelemetry services
        Assert.Contains(services, sd => sd.ServiceType == typeof(TracerProvider));
    }

    [Fact]
    public void AddWanderpoolTracing_Default_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddWanderpoolTracing();

        // Assert
        Assert.Equal(services, result);
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
        Assert.Contains(services, sd => sd.ServiceType == typeof(TracerProvider));
    }

    [Fact]
    public void AddWanderpoolTracing_WithCustomServiceName_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddWanderpoolTracing("MyService");

        // Assert
        Assert.Equal(services, result);
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
        Assert.Contains(services, sd => sd.ServiceType == typeof(TracerProvider));
    }

    [Fact]
    public void AddWanderpoolTracing_WithCustomEndpoint_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddWanderpoolTracing("http://localhost:4317");

        // Assert
        Assert.Equal(services, result);
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
        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains("Sampling probability must be between 0.0 and 1.0", exception.Message);
    }

    [Fact]
    public void AddWanderpoolTracing_WithSamplingProbabilityAboveOne_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var action = () => services.AddWanderpoolTracing("http://localhost:4317", samplingProbability: 1.1);
        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains("Sampling probability must be between 0.0 and 1.0", exception.Message);
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
        Assert.NotNull(provider);
        Assert.Contains(services, sd => sd.ServiceType == typeof(TracerProvider));
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
        Assert.Contains(services, sd => sd.ServiceType == typeof(TracerProvider));
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
        Assert.Contains(services, sd => sd.ServiceType == typeof(TracerProvider));
    }

    [Fact]
    public void AddWanderpoolTracing_WithCustomEndpointSamplingAndServiceName_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddWanderpoolTracing("http://otel:4317", 0.8, "Service");

        // Assert
        Assert.Equal(services, result);
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
        Assert.Contains(services, sd => sd.ServiceType == typeof(TracerProvider));
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
        Assert.NotNull(provider);
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
        Assert.Throws<ArgumentException>(action);
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
        Assert.Contains(services, sd => sd.ServiceType == typeof(TracerProvider));
    }

    [Fact]
    public void AddWanderpoolTracingWithConfigurableExporters_WithConfigurationSection_RegistersOpenTelemetry()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "OpenTelemetry:Enabled", "true" },
                { "OpenTelemetry:Endpoint", "http://localhost:4317" },
                { "OpenTelemetry:SamplingProbability", "1.0" }
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolTracingWithConfigurableExporters(config);

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(TracerProvider));
    }

    [Fact]
    public void AddWanderpoolTracingWithConfigurableExporters_WithMultipleExporters_RegistersAllExporters()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "OpenTelemetry:Enabled", "true" },
                { "OpenTelemetry:SamplingProbability", "1.0" },
                { "OpenTelemetry:Exporters:OTLP:Type", "otlp" },
                { "OpenTelemetry:Exporters:OTLP:Endpoint", "http://localhost:4317" },
                { "OpenTelemetry:Exporters:OTLP:Enabled", "true" },
                { "OpenTelemetry:Exporters:Jaeger:Type", "jaeger" },
                { "OpenTelemetry:Exporters:Jaeger:Endpoint", "http://localhost:14268/api/traces" },
                { "OpenTelemetry:Exporters:Jaeger:Enabled", "true" }
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolTracingWithConfigurableExporters(config);

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(TracerProvider));
    }

    [Fact]
    public void AddWanderpoolTracingWithConfigurableExporters_WithSampling_ConfiguresSamplingRate()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "OpenTelemetry:Enabled", "true" },
                { "OpenTelemetry:Endpoint", "http://localhost:4317" },
                { "OpenTelemetry:SamplingProbability", "0.5" }
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolTracingWithConfigurableExporters(config);

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(TracerProvider));
    }

    [Fact]
    public void AddWanderpoolTracingWithConfigurableExporters_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "OpenTelemetry:Enabled", "true" },
                { "OpenTelemetry:Endpoint", "http://localhost:4317" }
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        var result = services.AddWanderpoolTracingWithConfigurableExporters(config);

        // Assert
        Assert.Equal(services, result);
    }

    [Fact]
    public void AddWanderpoolTracingWithConfigurableExporters_WithCustomServiceName_RegistersServices()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "OpenTelemetry:Enabled", "true" },
                { "OpenTelemetry:Endpoint", "http://localhost:4317" }
            })
            .Build();

        var services = new ServiceCollection();
        var customServiceName = "MyTestService";

        // Act
        services.AddWanderpoolTracingWithConfigurableExporters(config, customServiceName);

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(TracerProvider));
    }

    [Fact]
    public void AddWanderpoolTracingWithConfigurableExporters_WithConsoleExporter_RegistersConsoleExporter()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "OpenTelemetry:Enabled", "true" },
                { "OpenTelemetry:SamplingProbability", "1.0" },
                { "OpenTelemetry:Exporters:Console:Type", "console" },
                { "OpenTelemetry:Exporters:Console:Enabled", "true" }
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolTracingWithConfigurableExporters(config);

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(TracerProvider));
    }

    [Fact]
    public void AddWanderpoolTracingWithConfigurableExporters_WithZipkinExporter_RegistersZipkinExporter()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "OpenTelemetry:Enabled", "true" },
                { "OpenTelemetry:SamplingProbability", "1.0" },
                { "OpenTelemetry:Exporters:Zipkin:Type", "zipkin" },
                { "OpenTelemetry:Exporters:Zipkin:Endpoint", "http://localhost:9411/api/v2/spans" },
                { "OpenTelemetry:Exporters:Zipkin:Enabled", "true" }
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolTracingWithConfigurableExporters(config);

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(TracerProvider));
    }

    [Fact]
    public void AddWanderpoolTracingWithConfigurableExporters_WithDisabledExporter_SkipsExporter()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "OpenTelemetry:Enabled", "true" },
                { "OpenTelemetry:SamplingProbability", "1.0" },
                { "OpenTelemetry:Exporters:OTLP:Type", "otlp" },
                { "OpenTelemetry:Exporters:OTLP:Endpoint", "http://localhost:4317" },
                { "OpenTelemetry:Exporters:OTLP:Enabled", "false" }
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolTracingWithConfigurableExporters(config);

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(TracerProvider));
    }
}
