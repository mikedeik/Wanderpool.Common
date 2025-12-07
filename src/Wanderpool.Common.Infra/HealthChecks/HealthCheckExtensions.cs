using Microsoft.Extensions.DependencyInjection;

namespace Wanderpool.Common.Infra.HealthChecks;

/// <summary>
/// Extension methods for registering and configuring health checks.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds Wanderpool health checks to the service collection.
    /// Includes default health checks for common scenarios.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddWanderpoolHealthChecks(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Add default health checks
        services.AddHealthChecks();

        return services;
    }

    /// <summary>
    /// Adds Wanderpool health checks with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action for health checks.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddWanderpoolHealthChecks(
        this IServiceCollection services,
        Action<IHealthChecksBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        // Add health checks and configure with the provided action
        var builder = services.AddHealthChecks();
        configure(builder);

        return services;
    }
}
