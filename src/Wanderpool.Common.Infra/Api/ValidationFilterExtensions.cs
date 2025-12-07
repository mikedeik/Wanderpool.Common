using Microsoft.AspNetCore.Builder;

namespace Wanderpool.Common.Infra.Api;

/// <summary>
/// Extension methods for registering validation filters on route handlers.
/// </summary>
public static class ValidationFilterExtensions
{
    /// <summary>
    /// Adds automatic validation filter to a route handler.
    /// The filter will validate incoming requests using FluentValidation and return 400 Bad Request if validation fails.
    /// </summary>
    /// <typeparam name="T">The request type to validate.</typeparam>
    /// <param name="builder">The route handler builder.</param>
    /// <returns>The route handler builder for chaining.</returns>
    public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddEndpointFilter<ValidationFilter<T>>();
    }
}
