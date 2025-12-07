using System.Diagnostics;

namespace Wanderpool.Common.Infra.Telemetry;

/// <summary>
/// Provides access to the custom Wanderpool ActivitySource for distributed tracing.
/// </summary>
public sealed class ActivitySourceProvider
{
    /// <summary>
    /// Gets the custom Wanderpool.Common ActivitySource.
    /// </summary>
    public ActivitySource ActivitySource { get; }

    /// <summary>
    /// Initializes a new instance of the ActivitySourceProvider class.
    /// </summary>
    public ActivitySourceProvider()
    {
        ActivitySource = new ActivitySource("Wanderpool.Common", "1.0.0");
    }
}
