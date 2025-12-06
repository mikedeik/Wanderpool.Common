using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;

namespace Wanderpool.Common.Infra.Telemetry;

/// <summary>
/// Factory for registering OpenTelemetry exporters based on configuration.
/// Supports: OTLP, Jaeger, Zipkin, and Console exporters.
/// </summary>
public static class ExporterFactory
{
    /// <summary>
    /// Registers an exporter with the trace provider builder based on configuration.
    /// </summary>
    /// <param name="traceBuilder">The trace provider builder.</param>
    /// <param name="config">The exporter configuration.</param>
    /// <returns>The trace provider builder for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when exporter type is not supported.</exception>
    public static TracerProviderBuilder AddConfiguredExporter(
        this TracerProviderBuilder traceBuilder,
        ExporterConfiguration config)
    {
        if (!config.Enabled)
        {
            return traceBuilder;
        }

        return config.Type.ToLowerInvariant() switch
        {
            "otlp" => AddOtlpExporter(traceBuilder, config),
            "jaeger" => AddJaegerExporter(traceBuilder, config),
            "zipkin" => AddZipkinExporter(traceBuilder, config),
            "console" => AddConsoleExporter(traceBuilder, config),
            _ => throw new ArgumentException(
                $"Unsupported exporter type '{config.Type}'. Supported types: otlp, jaeger, zipkin, console.",
                nameof(config))
        };
    }

    /// <summary>
    /// Registers OTLP (OpenTelemetry Protocol) exporter.
    /// </summary>
    private static TracerProviderBuilder AddOtlpExporter(
        TracerProviderBuilder traceBuilder,
        ExporterConfiguration config)
    {
        return traceBuilder.AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(config.Endpoint);
        });
    }

    /// <summary>
    /// Registers Jaeger exporter.
    /// </summary>
    private static TracerProviderBuilder AddJaegerExporter(
        TracerProviderBuilder traceBuilder,
        ExporterConfiguration config)
    {
        return traceBuilder.AddJaegerExporter(options =>
        {
            options.Endpoint = new Uri(config.Endpoint);
        });
    }

    /// <summary>
    /// Registers Zipkin exporter.
    /// </summary>
    private static TracerProviderBuilder AddZipkinExporter(
        TracerProviderBuilder traceBuilder,
        ExporterConfiguration config)
    {
        return traceBuilder.AddZipkinExporter(options =>
        {
            options.Endpoint = new Uri(config.Endpoint);
        });
    }

    /// <summary>
    /// Registers Console exporter for debugging.
    /// </summary>
    private static TracerProviderBuilder AddConsoleExporter(
        TracerProviderBuilder traceBuilder,
        ExporterConfiguration config)
    {
        return traceBuilder.AddConsoleExporter();
    }

    /// <summary>
    /// Registers exporters from configuration.
    /// If no exporters are configured, defaults to OTLP exporter.
    /// </summary>
    /// <param name="traceBuilder">The trace provider builder.</param>
    /// <param name="config">The OpenTelemetry configuration containing exporters.</param>
    /// <returns>The trace provider builder for method chaining.</returns>
    public static TracerProviderBuilder AddConfiguredExporters(
        this TracerProviderBuilder traceBuilder,
        OpenTelemetryConfiguration config)
    {
        if (config.Exporters.Count == 0)
        {
            // Default to OTLP exporter if none specified
            return AddOtlpExporter(traceBuilder, new ExporterConfiguration
            {
                Type = "otlp",
                Endpoint = config.Endpoint
            });
        }

        foreach (var exporter in config.Exporters.Values)
        {
            traceBuilder.AddConfiguredExporter(exporter);
        }

        return traceBuilder;
    }
}
