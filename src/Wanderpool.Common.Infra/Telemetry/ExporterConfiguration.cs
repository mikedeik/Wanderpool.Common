namespace Wanderpool.Common.Infra.Telemetry;

/// <summary>
/// Configuration for individual exporters in OpenTelemetry tracing.
/// </summary>
public class ExporterConfiguration
{
    /// <summary>
    /// The type of exporter to use: "otlp", "jaeger", "zipkin", "console".
    /// </summary>
    public string Type { get; set; } = "otlp";

    /// <summary>
    /// Exporter endpoint URL.
    /// - OTLP: "http://localhost:4317" (gRPC) or "http://localhost:4318" (HTTP)
    /// - Jaeger: "http://localhost:6831/send" or "http://localhost:14268/api/traces"
    /// - Zipkin: "http://localhost:9411/api/v2/spans"
    /// </summary>
    public string Endpoint { get; set; } = "http://localhost:4317";

    /// <summary>
    /// Whether this exporter is enabled. Defaults to true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Maximum number of spans to batch before sending. Defaults to 512.
    /// </summary>
    public int BatchSize { get; set; } = 512;

    /// <summary>
    /// Maximum time to wait before sending a batch, in milliseconds. Defaults to 5000 (5 seconds).
    /// </summary>
    public int TimeoutMs { get; set; } = 5000;

    /// <summary>
    /// Maximum number of spans to keep in memory before dropping. Defaults to 2048.
    /// </summary>
    public int MaxQueueSize { get; set; } = 2048;
}
