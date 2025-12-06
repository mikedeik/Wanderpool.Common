namespace Wanderpool.Common.Infra.Telemetry;

/// <summary>
/// Configuration for OpenTelemetry metrics collection.
/// </summary>
public class MetricsConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string Name = "OpenTelemetryMetrics";

    /// <summary>
    /// Whether to enable metrics collection. Defaults to true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Enable ASP.NET Core metrics (request duration, count, etc.). Defaults to true.
    /// </summary>
    public bool EnableAspNetCoreMetrics { get; set; } = true;

    /// <summary>
    /// Enable HttpClient metrics (outbound request stats). Defaults to true.
    /// </summary>
    public bool EnableHttpClientMetrics { get; set; } = true;

    /// <summary>
    /// Enable runtime metrics (GC, thread pool, etc.). Defaults to true.
    /// </summary>
    public bool EnableRuntimeMetrics { get; set; } = true;

    /// <summary>
    /// OTLP (OpenTelemetry Protocol) exporter endpoint URL.
    /// Example: "http://localhost:4317"
    /// </summary>
    public string OtlpEndpoint { get; set; } = "http://localhost:4317";

    /// <summary>
    /// Metrics export interval in seconds. Defaults to 60.
    /// </summary>
    public int ExportIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Maximum number of metrics in memory before export. Defaults to 2000.
    /// </summary>
    public int MaxMetricsBufferSize { get; set; } = 2000;
}
