namespace Wanderpool.Common.Infra.Telemetry;

/// <summary>
/// Configuration for correlation ID tracking.
/// Maps to appsettings.json "CorrelationId" section.
/// </summary>
public class CorrelationIdConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string Name = "CorrelationId";

    /// <summary>
    /// The HTTP header name to read correlation ID from.
    /// If the header is not present, a new correlation ID will be generated.
    /// Default: "X-Correlation-Id"
    /// </summary>
    public string HeaderName { get; set; } = "X-Correlation-Id";

    /// <summary>
    /// The key name used to store correlation ID in HttpContext.Items.
    /// Default: "CorrelationId"
    /// </summary>
    public string ContextItemKey { get; set; } = "CorrelationId";

    /// <summary>
    /// Whether to include correlation ID in response headers.
    /// Default: true
    /// </summary>
    public bool IncludeInResponseHeader { get; set; } = true;

    /// <summary>
    /// The format for generating new correlation IDs if not provided.
    /// Valid values: "D" (default GUID), "N" (no hyphens), "B" (braced)
    /// Default: "D"
    /// </summary>
    public string GuidFormat { get; set; } = "D";
}
