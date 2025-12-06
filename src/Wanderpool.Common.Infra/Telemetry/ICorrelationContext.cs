namespace Wanderpool.Common.Infra.Telemetry;

/// <summary>
/// Provides correlation ID context for tracking requests across services and logs.
/// </summary>
public interface ICorrelationContext
{
    /// <summary>
    /// Gets the current correlation ID.
    /// This ID is generated for each request or extracted from the X-Correlation-Id header.
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    /// Gets the X-Correlation-Id header name.
    /// </summary>
    string HeaderName => "X-Correlation-Id";
}
