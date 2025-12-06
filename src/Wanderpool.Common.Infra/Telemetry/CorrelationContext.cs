namespace Wanderpool.Common.Infra.Telemetry;

/// <summary>
/// Default implementation of correlation context for request tracking.
/// Provides access to the correlation ID associated with the current request.
/// </summary>
public class CorrelationContext : ICorrelationContext
{
    /// <summary>
    /// Creates a new instance of CorrelationContext.
    /// </summary>
    /// <param name="correlationId">The correlation ID to track with.</param>
    public CorrelationContext(string correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
            throw new ArgumentException("Correlation ID cannot be null or empty.", nameof(correlationId));

        CorrelationId = correlationId;
    }

    /// <summary>
    /// Gets the correlation ID for the current request context.
    /// </summary>
    public string CorrelationId { get; }
}
