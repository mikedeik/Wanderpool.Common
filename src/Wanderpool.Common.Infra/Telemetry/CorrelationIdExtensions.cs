using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace Wanderpool.Common.Infra.Telemetry;

/// <summary>
/// Extension methods for configuring correlation ID tracking in the request pipeline.
/// </summary>
public static class CorrelationIdExtensions
{
    /// <summary>
    /// Adds correlation ID context service to the dependency injection container with hardcoded configuration.
    /// This must be called before UseCorrelationId middleware is used.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// Uses hardcoded defaults. For configurable correlation ID settings, use
    /// AddWanderpoolCorrelationIdWithConfiguration() instead.
    /// </remarks>
    public static IServiceCollection AddWanderpoolCorrelationId(this IServiceCollection services)
    {
        var config = new CorrelationIdConfiguration();
        return AddWanderpoolCorrelationIdInternal(services, config);
    }

    /// <summary>
    /// Adds correlation ID context service with configuration from appsettings.json.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// Reads configuration from appsettings.json "CorrelationId" section.
    /// This is the recommended approach for production applications.
    /// </remarks>
    public static IServiceCollection AddWanderpoolCorrelationIdWithConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var config = new CorrelationIdConfiguration();
        configuration.GetSection(CorrelationIdConfiguration.Name).Bind(config);
        return AddWanderpoolCorrelationIdInternal(services, config);
    }

    /// <summary>
    /// Internal implementation of correlation ID configuration.
    /// </summary>
    private static IServiceCollection AddWanderpoolCorrelationIdInternal(
        IServiceCollection services,
        CorrelationIdConfiguration config)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICorrelationContext>(provider =>
        {
            var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
            var httpContext = httpContextAccessor.HttpContext;

            var correlationId = httpContext?.Items[config.ContextItemKey] as string
                ?? Guid.NewGuid().ToString(config.GuidFormat);

            return new CorrelationContext(correlationId);
        });

        // Store configuration in services for middleware access
        services.AddSingleton(Options.Create(config));

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
