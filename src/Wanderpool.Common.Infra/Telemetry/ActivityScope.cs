using System.Diagnostics;

namespace Wanderpool.Common.Infra.Telemetry;

/// <summary>
/// Implementation of IActivityScope for managing OpenTelemetry Activity lifecycle.
/// Creates and manages a span (Activity) for distributed tracing operations.
/// </summary>
public class ActivityScope : IActivityScope
{
    private readonly ActivitySource _activitySource;
    private bool _disposed = false;

    /// <summary>
    /// Gets the underlying Activity (span) managed by this scope.
    /// </summary>
    public Activity Activity { get; }

    /// <summary>
    /// Initializes a new instance of the ActivityScope class.
    /// </summary>
    /// <param name="activitySource">The ActivitySource to create activities from.</param>
    /// <param name="operationName">The name of the operation for the activity.</param>
    /// <exception cref="ArgumentNullException">Thrown when activitySource or operationName is null.</exception>
    /// <exception cref="ArgumentException">Thrown when operationName is empty or whitespace.</exception>
    public ActivityScope(ActivitySource activitySource, string operationName)
    {
        if (activitySource == null)
        {
            throw new ArgumentNullException(nameof(activitySource));
        }

        if (string.IsNullOrWhiteSpace(operationName))
        {
            throw new ArgumentException("Operation name cannot be null or empty.", nameof(operationName));
        }

        _activitySource = activitySource;
        Activity = _activitySource.StartActivity(operationName) ?? new Activity(operationName);
    }

    /// <summary>
    /// Adds a tag (attribute) to the current activity.
    /// </summary>
    /// <param name="key">The tag key.</param>
    /// <param name="value">The tag value.</param>
    /// <returns>This scope for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when key is null or empty.</exception>
    public IActivityScope AddTag(string key, object? value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Tag key cannot be null or empty.", nameof(key));
        }

        Activity.SetTag(key, value);
        return this;
    }

    /// <summary>
    /// Records an exception on the current activity.
    /// </summary>
    /// <param name="exception">The exception to record.</param>
    /// <returns>This scope for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when exception is null.</exception>
    public IActivityScope RecordException(Exception exception)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        // Add exception event to activity (OpenTelemetry standard)
        var tagsCollection = new ActivityTagsCollection
        {
            { "exception.type", exception.GetType().FullName },
            { "exception.message", exception.Message },
            { "exception.stacktrace", exception.StackTrace }
        };

        Activity.AddEvent(new ActivityEvent("exception", tags: tagsCollection));
        return this;
    }

    /// <summary>
    /// Sets the status of the current activity.
    /// </summary>
    /// <param name="statusCode">The status code to set.</param>
    /// <param name="description">Optional status description.</param>
    /// <returns>This scope for method chaining.</returns>
    public IActivityScope SetStatus(ActivityStatusCode statusCode, string? description = null)
    {
        Activity.SetStatus(statusCode, description);
        return this;
    }

    /// <summary>
    /// Disposes the activity scope and completes the activity.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Activity?.Dispose();
        _disposed = true;
    }
}
