namespace Wanderpool.Common.Infra;

/// <summary>
/// Unified configuration options for Wanderpool infrastructure components.
/// </summary>
public class WanderpoolOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string Name = "Wanderpool";

    /// <summary>
    /// Enable or disable logging infrastructure.
    /// </summary>
    public bool EnableLogging { get; set; } = true;

    /// <summary>
    /// Enable or disable tracing (OpenTelemetry).
    /// </summary>
    public bool EnableTracing { get; set; } = true;

    /// <summary>
    /// Enable or disable metrics (OpenTelemetry Metrics).
    /// </summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// Enable or disable exception handling middleware.
    /// </summary>
    public bool EnableExceptionHandling { get; set; } = true;

    /// <summary>
    /// Enable or disable health checks.
    /// </summary>
    public bool EnableHealthChecks { get; set; } = true;

    /// <summary>
    /// Enable or disable correlation ID tracking.
    /// </summary>
    public bool EnableCorrelationId { get; set; } = true;

    /// <summary>
    /// Enable or disable request logging.
    /// </summary>
    public bool EnableRequestLogging { get; set; } = true;

    /// <summary>
    /// Service name for telemetry (tracing, metrics).
    /// </summary>
    public string ServiceName { get; set; } = "Wanderpool.Service";

    /// <summary>
    /// Service version for telemetry.
    /// </summary>
    public string ServiceVersion { get; set; } = "1.0.0";
}
