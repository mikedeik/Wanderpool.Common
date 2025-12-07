using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

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

    /// <summary>
    /// Applies common filters to all endpoints in the group.
    /// </summary>
    /// <typeparam name="T">The filter type implementing IEndpointFilter.</typeparam>
    /// <param name="app">The route group builder.</param>
    /// <returns>The route group builder for chaining.</returns>
    public static RouteGroupBuilder WithCommonFilters<T>(this RouteGroupBuilder app) where T : IEndpointFilter
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.AddEndpointFilter<T>();
    }

    /// <summary>
    /// Adds authentication requirement to all endpoints in the group.
    /// </summary>
    /// <param name="app">The route group builder.</param>
    /// <returns>The route group builder for chaining.</returns>
    public static RouteGroupBuilder WithAuthentication(this RouteGroupBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.RequireAuthorization();
    }

    /// <summary>
    /// Adds tags to all endpoints in the group for OpenAPI documentation.
    /// </summary>
    /// <param name="app">The route group builder.</param>
    /// <param name="tags">The tags to add to group endpoints.</param>
    /// <returns>The route group builder for chaining.</returns>
    public static RouteGroupBuilder WithTags(this RouteGroupBuilder app, params string[] tags)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(tags);

        foreach (var tag in tags)
        {
            if (!string.IsNullOrWhiteSpace(tag))
            {
                app.WithTags(tag);
            }
        }

        return app;
    }
}
