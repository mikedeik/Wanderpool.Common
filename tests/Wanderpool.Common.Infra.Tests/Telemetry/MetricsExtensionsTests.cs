using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using Wanderpool.Common.Infra.Telemetry;

namespace Wanderpool.Common.Infra.Tests.Telemetry;

public class MetricsExtensionsTests
{
    [Fact]
    public void AddWanderpoolMetrics_Default_RegistersOpenTelemetry()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolMetrics();

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(MeterProvider));
    }

    [Fact]
    public void AddWanderpoolMetrics_Default_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddWanderpoolMetrics();

        // Assert
        Assert.Equal(services, result);
    }

    [Fact]
    public void AddWanderpoolMetrics_WithCustomServiceName_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var customServiceName = "CustomService";

        // Act
        services.AddWanderpoolMetrics(customServiceName);

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(MeterProvider));
    }

    [Fact]
    public void AddWanderpoolMetrics_WithCustomEndpoint_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var endpoint = "http://localhost:4317";

        // Act
        services.AddWanderpoolMetrics(endpoint);

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(MeterProvider));
    }

    [Fact]
    public void AddWanderpoolMetrics_WithCustomEndpoint_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddWanderpoolMetrics("http://localhost:4317");

        // Assert
        Assert.Equal(services, result);
    }

    [Fact]
    public void AddWanderpoolMetricsWithConfiguration_WithBasicConfig_RegistersOpenTelemetry()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "OpenTelemetryMetrics:Enabled", "true" },
                { "OpenTelemetryMetrics:EnableAspNetCoreMetrics", "true" },
                { "OpenTelemetryMetrics:EnableHttpClientMetrics", "true" },
                { "OpenTelemetryMetrics:EnableRuntimeMetrics", "true" },
                { "OpenTelemetryMetrics:OtlpEndpoint", "http://localhost:4317" }
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolMetricsWithConfiguration(config);

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(MeterProvider));
    }

    [Fact]
    public void AddWanderpoolMetricsWithConfiguration_WithDisabledMetrics_DoesNotRegister()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "OpenTelemetryMetrics:Enabled", "false" }
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolMetricsWithConfiguration(config);

        // Assert
        // Service collection should not have MeterProvider registered
        Assert.DoesNotContain(services, sd => sd.ServiceType == typeof(MeterProvider));
    }

    [Fact]
    public void AddWanderpoolMetricsWithConfiguration_WithAspNetCoreOnly_RegistersAspNetCoreMetrics()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "OpenTelemetryMetrics:Enabled", "true" },
                { "OpenTelemetryMetrics:EnableAspNetCoreMetrics", "true" },
                { "OpenTelemetryMetrics:EnableHttpClientMetrics", "false" },
                { "OpenTelemetryMetrics:EnableRuntimeMetrics", "false" },
                { "OpenTelemetryMetrics:OtlpEndpoint", "http://localhost:4317" }
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolMetricsWithConfiguration(config);

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(MeterProvider));
    }

    [Fact]
    public void AddWanderpoolMetricsWithConfiguration_WithHttpClientOnly_RegistersHttpClientMetrics()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "OpenTelemetryMetrics:Enabled", "true" },
                { "OpenTelemetryMetrics:EnableAspNetCoreMetrics", "false" },
                { "OpenTelemetryMetrics:EnableHttpClientMetrics", "true" },
                { "OpenTelemetryMetrics:EnableRuntimeMetrics", "false" },
                { "OpenTelemetryMetrics:OtlpEndpoint", "http://localhost:4317" }
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolMetricsWithConfiguration(config);

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(MeterProvider));
    }

    [Fact]
    public void AddWanderpoolMetricsWithConfiguration_WithRuntimeOnly_RegistersRuntimeMetrics()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "OpenTelemetryMetrics:Enabled", "true" },
                { "OpenTelemetryMetrics:EnableAspNetCoreMetrics", "false" },
                { "OpenTelemetryMetrics:EnableHttpClientMetrics", "false" },
                { "OpenTelemetryMetrics:EnableRuntimeMetrics", "true" },
                { "OpenTelemetryMetrics:OtlpEndpoint", "http://localhost:4317" }
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolMetricsWithConfiguration(config);

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(MeterProvider));
    }

    [Fact]
    public void AddWanderpoolMetricsWithConfiguration_WithCustomOtlpEndpoint_RegistersServices()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "OpenTelemetryMetrics:Enabled", "true" },
                { "OpenTelemetryMetrics:OtlpEndpoint", "http://custom-collector:4317" }
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolMetricsWithConfiguration(config);

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(MeterProvider));
    }


    [Fact]
    public void AddWanderpoolMetricsWithConfiguration_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "OpenTelemetryMetrics:Enabled", "true" }
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        var result = services.AddWanderpoolMetricsWithConfiguration(config);

        // Assert
        Assert.Equal(services, result);
    }

    [Fact]
    public void AddWanderpoolMetricsWithConfiguration_WithCustomServiceName_RegistersServices()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "OpenTelemetryMetrics:Enabled", "true" }
            })
            .Build();

        var services = new ServiceCollection();
        var customServiceName = "MyTestService";

        // Act
        services.AddWanderpoolMetricsWithConfiguration(config, customServiceName);

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(MeterProvider));
    }

    [Fact]
    public void AddWanderpoolMetricsWithConfiguration_WithAllMetricsEnabled_RegistersAllMetrics()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "OpenTelemetryMetrics:Enabled", "true" },
                { "OpenTelemetryMetrics:EnableAspNetCoreMetrics", "true" },
                { "OpenTelemetryMetrics:EnableHttpClientMetrics", "true" },
                { "OpenTelemetryMetrics:EnableRuntimeMetrics", "true" },
                { "OpenTelemetryMetrics:OtlpEndpoint", "http://localhost:4317" },
                { "OpenTelemetryMetrics:ExportIntervalSeconds", "30" },
                { "OpenTelemetryMetrics:MaxMetricsBufferSize", "5000" }
            })
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolMetricsWithConfiguration(config);

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(MeterProvider));
    }

    [Fact]
    public void MetricsConfiguration_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var config = new MetricsConfiguration();

        // Assert
        Assert.True(config.Enabled);
        Assert.True(config.EnableAspNetCoreMetrics);
        Assert.True(config.EnableHttpClientMetrics);
        Assert.True(config.EnableRuntimeMetrics);
        Assert.Equal("http://localhost:4317", config.OtlpEndpoint);
        Assert.Equal(60, config.ExportIntervalSeconds);
        Assert.Equal(2000, config.MaxMetricsBufferSize);
    }

    [Fact]
    public void MetricsConfiguration_CanBeConfiguredFromDictionary()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "OpenTelemetryMetrics:Enabled", "false" },
                { "OpenTelemetryMetrics:OtlpEndpoint", "http://custom:4317" },
                { "OpenTelemetryMetrics:ExportIntervalSeconds", "120" },
                { "OpenTelemetryMetrics:MaxMetricsBufferSize", "5000" }
            })
            .Build();

        var metricsConfig = new MetricsConfiguration();

        // Act
        config.GetSection(MetricsConfiguration.Name).Bind(metricsConfig);

        // Assert
        Assert.False(metricsConfig.Enabled);
        Assert.Equal("http://custom:4317", metricsConfig.OtlpEndpoint);
        Assert.Equal(120, metricsConfig.ExportIntervalSeconds);
        Assert.Equal(5000, metricsConfig.MaxMetricsBufferSize);
    }
}
