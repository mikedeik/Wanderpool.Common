using Microsoft.AspNetCore.Builder;

namespace Wanderpool.Common.Infra.Exceptions;

/// <summary>
/// Extension methods for configuring global exception handling middleware.
/// </summary>
public static class GlobalExceptionHandlingExtensions
{
    /// <summary>
    /// Adds the Wanderpool global exception handling middleware to the request pipeline.
    /// This middleware catches all unhandled exceptions and returns them in a consistent
    /// ApiResponseEnvelope format with appropriate HTTP status codes.
    ///
    /// In production mode, detailed error messages for 5xx errors are hidden.
    /// In development mode, full error details are shown for debugging.
    /// </summary>
    /// <param name="app">The WebApplication builder.</param>
    /// <returns>The WebApplication builder for method chaining.</returns>
    /// <remarks>
    /// This middleware should be registered early in the pipeline (typically after correlation ID middleware)
    /// to ensure it can catch exceptions from all subsequent middleware.
    ///
    /// Example usage:
    /// <code>
    /// var app = builder.Build();
    /// app.UseWanderpoolExceptionHandling();
    /// app.UseRouting();
    /// app.MapControllers();
    /// </code>
    /// </remarks>
    public static WebApplication UseWanderpoolExceptionHandling(this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        return app;
    }
}
