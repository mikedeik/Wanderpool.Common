using Microsoft.Extensions.DependencyInjection;
using Wanderpool.Common.Infra.Telemetry;
using Wanderpool.Common.Infra.HealthChecks;

namespace Wanderpool.Common.Infra;

/// <summary>
/// Extension methods for registering all Wanderpool infrastructure services.
/// </summary>
public static class WanderpoolInfrastructureExtensions
{
    /// <summary>
    /// Adds all Wanderpool infrastructure services to the dependency injection container.
    /// Services that require WebApplicationBuilder configuration are excluded from this method
    /// and should be configured in Program.cs separately.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action for Wanderpool options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// This method registers DI-only services:
    /// - Tracing (AddWanderpoolTracing)
    /// - Metrics (AddWanderpoolMetrics)
    /// - Health checks (AddWanderpoolHealthChecks)
    /// - Correlation ID context (AddWanderpoolCorrelationId)
    ///
    /// Services requiring WebApplicationBuilder configuration must be called separately:
    /// - builder.AddWanderpoolLogging(serviceName) - for Serilog setup
    /// - app.UseWanderpoolExceptionHandling() - for exception middleware
    /// - app.UseWanderpoolCorrelationId() - for correlation ID middleware
    /// - app.UseWanderpoolRequestLogging() - for request logging middleware
    /// </remarks>
    public static IServiceCollection AddWanderpoolInfrastructure(
        this IServiceCollection services,
        Action<WanderpoolOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new WanderpoolOptions();
        configure(options);

        // Validate options
        ValidateOptions(options);

        // Register tracing
        if (options.EnableTracing)
        {
            services.AddWanderpoolTracing();
        }

        // Register metrics
        if (options.EnableMetrics)
        {
            services.AddWanderpoolMetrics();
        }

        // Register health checks
        if (options.EnableHealthChecks)
        {
            services.AddWanderpoolHealthChecks();
        }

        // Register correlation ID context
        if (options.EnableCorrelationId)
        {
            services.AddWanderpoolCorrelationId();
        }

        // Note: Logging, Exception Handling, and Request Logging require WebApplicationBuilder
        // or WebApplication and should be configured separately in Program.cs

        return services;
    }

    /// <summary>
    /// Validates the Wanderpool options and throws if invalid.
    /// </summary>
    /// <param name="options">The options to validate.</param>
    /// <exception cref="ArgumentException">Thrown when options are invalid.</exception>
    private static void ValidateOptions(WanderpoolOptions options)
    {
        if (string.IsNullOrEmpty(options.ServiceName))
        {
            throw new ArgumentException("ServiceName cannot be null or empty.", nameof(options.ServiceName));
        }

        if (string.IsNullOrEmpty(options.ServiceVersion))
        {
            throw new ArgumentException("ServiceVersion cannot be null or empty.", nameof(options.ServiceVersion));
        }
    }
}
