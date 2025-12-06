using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Wanderpool.Common.Infra.Telemetry;

/// <summary>
/// Middleware for extracting or generating correlation IDs for request tracking.
/// Ensures each request has a unique correlation ID for distributed tracing.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderName = "X-Correlation-Id";

    /// <summary>
    /// Creates a new instance of CorrelationIdMiddleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Invokes the middleware to extract or generate correlation ID.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        // Try to extract correlation ID from request headers
        var correlationId = ExtractCorrelationId(context.Request.Headers);

        // If not found, generate a new one
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = GenerateCorrelationId();
        }

        // Store in HttpContext items for later access
        context.Items["CorrelationId"] = correlationId;

        // Add to response headers
        context.Response.Headers.Add(CorrelationIdHeaderName, correlationId);

        await _next(context);
    }

    /// <summary>
    /// Extracts the correlation ID from request headers.
    /// </summary>
    /// <param name="headers">The request headers.</param>
    /// <returns>The correlation ID if found; otherwise null.</returns>
    private static string? ExtractCorrelationId(IHeaderDictionary headers)
    {
        if (headers.TryGetValue(CorrelationIdHeaderName, out var correlationId))
        {
            var id = correlationId.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(id))
            {
                return id;
            }
        }

        return null;
    }

    /// <summary>
    /// Generates a new correlation ID.
    /// </summary>
    /// <returns>A new correlation ID in GUID format.</returns>
    private static string GenerateCorrelationId()
    {
        return Guid.NewGuid().ToString("D");
    }
}
