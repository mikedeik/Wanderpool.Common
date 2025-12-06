using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Wanderpool.Common.Infra.Telemetry;

/// <summary>
/// Extension methods for configuring correlation ID tracking in the request pipeline.
/// </summary>
public static class CorrelationIdExtensions
{
    /// <summary>
    /// Adds correlation ID context service to the dependency injection container.
    /// This must be called before UseCorrelationId middleware is used.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWanderpoolCorrelationId(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICorrelationContext>(provider =>
        {
            var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
            var httpContext = httpContextAccessor.HttpContext;

            var correlationId = httpContext?.Items["CorrelationId"] as string
                ?? Guid.NewGuid().ToString("D");

            return new CorrelationContext(correlationId);
        });

        // Add enricher for Serilog
        Log.Logger = Log.Logger.ForContext<CorrelationIdEnricher>();

        return services;
    }

    /// <summary>
    /// Adds the correlation ID middleware to the request pipeline.
    /// This middleware should be registered early in the pipeline to ensure
    /// correlation ID is available for all subsequent middleware.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for method chaining.</returns>
    public static WebApplication UseWanderpoolCorrelationId(this WebApplication app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        return app;
    }
}
