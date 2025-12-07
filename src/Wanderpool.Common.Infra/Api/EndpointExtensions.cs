using Microsoft.AspNetCore.Builder;

namespace Wanderpool.Common.Infra.Api;

/// <summary>
/// Extension methods for configuring endpoint groups and routes.
/// </summary>
public static class EndpointExtensions
{
    private const string ApiPrefix = "api";

    /// <summary>
    /// Creates an API endpoint group with the default "api" prefix.
    /// </summary>
    /// <param name="app">The route group builder.</param>
    /// <returns>The route group builder for chaining.</returns>
    public static RouteGroupBuilder MapApiGroup(this RouteGroupBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.MapGroup(ApiPrefix);
    }

    /// <summary>
    /// Creates an API endpoint group with a custom prefix.
    /// </summary>
    /// <param name="app">The route group builder.</param>
    /// <param name="prefix">The URL prefix for the group (e.g., "api/v1", "api/admin").</param>
    /// <returns>The route group builder for chaining.</returns>
    public static RouteGroupBuilder MapApiGroup(this RouteGroupBuilder app, string prefix)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(prefix);

        return app.MapGroup(prefix);
    }

    /// <summary>
    /// Creates a versioned API endpoint group (e.g., "api/v1", "api/v2").
    /// </summary>
    /// <param name="app">The route group builder.</param>
    /// <param name="version">The API version number.</param>
    /// <returns>The route group builder for chaining.</returns>
    public static RouteGroupBuilder MapVersionedApi(this RouteGroupBuilder app, int version)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (version < 1)
        {
            throw new ArgumentException("Version must be greater than 0", nameof(version));
        }

        return app.MapGroup($"{ApiPrefix}/v{version}");
    }
}
