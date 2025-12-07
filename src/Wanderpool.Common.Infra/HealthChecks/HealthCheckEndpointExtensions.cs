using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Wanderpool.Common.Infra.HealthChecks;

/// <summary>
/// Extension methods for mapping health check endpoints.
/// </summary>
public static class HealthCheckEndpointExtensions
{
    /// <summary>
    /// Maps Wanderpool health check endpoints.
    /// Configures /health/live (always returns 200) and /health/ready (returns 503 if unhealthy).
    /// </summary>
    /// <param name="app">The web application builder.</param>
    /// <returns>The web application builder for chaining.</returns>
    public static WebApplication MapWanderpoolHealthChecks(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // Configure /health/live endpoint - always returns 200 OK
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = WriteJsonResponse
        });

        // Configure /health/ready endpoint - returns 503 if unhealthy
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            ResponseWriter = WriteJsonResponse
        });

        return app;
    }

    /// <summary>
    /// Writes health check response in JSON format.
    /// </summary>
    private static async Task WriteJsonResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.ToDictionary(
                e => e.Key,
                e => new
                {
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds
                }
            ),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
