using System.Diagnostics;

namespace Wanderpool.Common.Infra.Telemetry;

/// <summary>
/// Abstraction for managing OpenTelemetry Activity (span) lifecycle in a scoped manner.
/// Provides a fluent API for adding tags, recording exceptions, and managing span status.
/// </summary>
public interface IActivityScope : IDisposable
{
    /// <summary>
    /// Gets the underlying Activity (span) managed by this scope.
    /// </summary>
    Activity Activity { get; }

    /// <summary>
    /// Adds a tag (attribute) to the current activity.
    /// </summary>
    /// <param name="key">The tag key.</param>
    /// <param name="value">The tag value.</param>
    /// <returns>This scope for method chaining.</returns>
    IActivityScope AddTag(string key, object? value);

    /// <summary>
    /// Records an exception on the current activity.
    /// </summary>
    /// <param name="exception">The exception to record.</param>
    /// <returns>This scope for method chaining.</returns>
    IActivityScope RecordException(Exception exception);

    /// <summary>
    /// Sets the status of the current activity.
    /// </summary>
    /// <param name="statusCode">The status code to set.</param>
    /// <param name="description">Optional status description.</param>
    /// <returns>This scope for method chaining.</returns>
    IActivityScope SetStatus(ActivityStatusCode statusCode, string? description = null);
}
