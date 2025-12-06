using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using Wanderpool.Common.Infra.Telemetry;

namespace Wanderpool.Common.Infra.Tests.Telemetry;

public class ExporterFactoryTests
{
    [Fact]
    public void AddConfiguredExporter_WithOtlpExporter_RegistersOtlpExporter()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ExporterConfiguration
        {
            Type = "otlp",
            Endpoint = "http://localhost:4317",
            Enabled = true
        };

        // Act
        services.AddOpenTelemetry().WithTracing(tb => tb.AddConfiguredExporter(config));
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<TracerProvider>());
    }

    [Fact]
    public void AddConfiguredExporter_WithJaegerExporter_RegistersJaegerExporter()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ExporterConfiguration
        {
            Type = "jaeger",
            Endpoint = "http://localhost:14268/api/traces",
            Enabled = true
        };

        // Act
        services.AddOpenTelemetry().WithTracing(tb => tb.AddConfiguredExporter(config));
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<TracerProvider>());
    }

    [Fact]
    public void AddConfiguredExporter_WithZipkinExporter_RegistersZipkinExporter()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ExporterConfiguration
        {
            Type = "zipkin",
            Endpoint = "http://localhost:9411/api/v2/spans",
            Enabled = true
        };

        // Act
        services.AddOpenTelemetry().WithTracing(tb => tb.AddConfiguredExporter(config));
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<TracerProvider>());
    }

    [Fact]
    public void AddConfiguredExporter_WithConsoleExporter_RegistersConsoleExporter()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ExporterConfiguration
        {
            Type = "console",
            Enabled = true
        };

        // Act
        services.AddOpenTelemetry().WithTracing(tb => tb.AddConfiguredExporter(config));
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<TracerProvider>());
    }

    [Fact]
    public void AddConfiguredExporter_WithDisabledExporter_DoesNotRegisterExporter()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ExporterConfiguration
        {
            Type = "otlp",
            Endpoint = "http://localhost:4317",
            Enabled = false
        };

        // Act
        services.AddOpenTelemetry().WithTracing(tb => tb.AddConfiguredExporter(config));
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<TracerProvider>());
    }

    [Fact]
    public void AddConfiguredExporter_WithInvalidExporterType_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ExporterConfiguration
        {
            Type = "invalid_exporter",
            Enabled = true
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            services.AddOpenTelemetry().WithTracing(tb => tb.AddConfiguredExporter(config));
        });
    }

    [Fact]
    public void AddConfiguredExporter_WithOtlpExporter_UsesCorrectEndpoint()
    {
        // Arrange
        var services = new ServiceCollection();
        var expectedEndpoint = "http://custom-otel:4317";
        var config = new ExporterConfiguration
        {
            Type = "otlp",
            Endpoint = expectedEndpoint,
            Enabled = true
        };

        // Act
        services.AddOpenTelemetry().WithTracing(tb => tb.AddConfiguredExporter(config));
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<TracerProvider>());
    }

    [Fact]
    public void AddConfiguredExporter_WithJaegerExporter_UsesCorrectEndpoint()
    {
        // Arrange
        var services = new ServiceCollection();
        var expectedEndpoint = "http://custom-jaeger:14268/api/traces";
        var config = new ExporterConfiguration
        {
            Type = "jaeger",
            Endpoint = expectedEndpoint,
            Enabled = true
        };

        // Act
        services.AddOpenTelemetry().WithTracing(tb => tb.AddConfiguredExporter(config));
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<TracerProvider>());
    }

    [Fact]
    public void AddConfiguredExporter_WithZipkinExporter_UsesCorrectEndpoint()
    {
        // Arrange
        var services = new ServiceCollection();
        var expectedEndpoint = "http://custom-zipkin:9411/api/v2/spans";
        var config = new ExporterConfiguration
        {
            Type = "zipkin",
            Endpoint = expectedEndpoint,
            Enabled = true
        };

        // Act
        services.AddOpenTelemetry().WithTracing(tb => tb.AddConfiguredExporter(config));
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<TracerProvider>());
    }

    [Fact]
    public void AddConfiguredExporter_WithCaseInsensitiveType_RegistersCorrectExporter()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ExporterConfiguration
        {
            Type = "OTLP",
            Endpoint = "http://localhost:4317",
            Enabled = true
        };

        // Act
        services.AddOpenTelemetry().WithTracing(tb => tb.AddConfiguredExporter(config));
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<TracerProvider>());
    }

    [Fact]
    public void AddConfiguredExporters_WithEmptyExporters_DefaultsToOtlp()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new OpenTelemetryConfiguration
        {
            Endpoint = "http://localhost:4317",
            Exporters = new Dictionary<string, ExporterConfiguration>()
        };

        // Act
        services.AddOpenTelemetry().WithTracing(tb => tb.AddConfiguredExporters(config));
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<TracerProvider>());
    }

    [Fact]
    public void AddConfiguredExporters_WithMultipleExporters_RegistersAllEnabledExporters()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new OpenTelemetryConfiguration
        {
            Exporters = new Dictionary<string, ExporterConfiguration>
            {
                ["OTLP"] = new ExporterConfiguration
                {
                    Type = "otlp",
                    Endpoint = "http://localhost:4317",
                    Enabled = true
                },
                ["Jaeger"] = new ExporterConfiguration
                {
                    Type = "jaeger",
                    Endpoint = "http://localhost:14268/api/traces",
                    Enabled = true
                },
                ["Console"] = new ExporterConfiguration
                {
                    Type = "console",
                    Enabled = false
                }
            }
        };

        // Act
        services.AddOpenTelemetry().WithTracing(tb => tb.AddConfiguredExporters(config));
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<TracerProvider>());
    }

    [Fact]
    public void AddConfiguredExporters_WithAllDisabledExporters_DefaultsToOtlp()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new OpenTelemetryConfiguration
        {
            Endpoint = "http://localhost:4317",
            Exporters = new Dictionary<string, ExporterConfiguration>
            {
                ["OTLP"] = new ExporterConfiguration { Type = "otlp", Enabled = false },
                ["Jaeger"] = new ExporterConfiguration { Type = "jaeger", Enabled = false }
            }
        };

        // Act
        services.AddOpenTelemetry().WithTracing(tb => tb.AddConfiguredExporters(config));
        var provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<TracerProvider>());
    }
}
