namespace Wanderpool.Common.Infra.Telemetry;

/// <summary>
/// Configuration for OpenTelemetry tracing exporters.
/// </summary>
public class OpenTelemetryConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string Name = "OpenTelemetry";

    /// <summary>
    /// OTLP (OpenTelemetry Protocol) exporter endpoint URL.
    /// Example: "http://localhost:4317" or "http://otel-collector:4317"
    /// </summary>
    public string Endpoint { get; set; } = "http://localhost:4317";

    /// <summary>
    /// Whether to enable tracing. Defaults to true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Sampling probability between 0.0 and 1.0. Defaults to 1.0 (100% sampling).
    /// </summary>
    public double SamplingProbability { get; set; } = 1.0;
}
