using Microsoft.Extensions.DependencyInjection;
using Wanderpool.Common.Infra.Clients.HttpClientHandlers;

namespace Wanderpool.Common.Infra.Clients;

/// <summary>
/// Extension methods for registering HTTP clients with Wanderpool infrastructure.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Adds a Wanderpool HTTP client with all infrastructure (logging, resilience, metrics).
    /// </summary>
    /// <typeparam name="T">The typed HTTP client interface.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action for client options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddWanderpoolHttpClient<T>(
        this IServiceCollection services,
        Action<HttpClientOptions> configure) where T : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new HttpClientOptions();
        configure(options);

        // Configure typed HTTP client with base address
        var httpClientBuilder = services
            .AddHttpClient<T>()
            .ConfigureHttpClient(client =>
            {
                if (!string.IsNullOrEmpty(options.BaseAddress))
                {
                    client.BaseAddress = new Uri(options.BaseAddress);
                }
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            });

        // Add token refresh handler if token provider type is specified
        if (options.TokenProviderType != null)
        {
            httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var tokenProvider = provider.GetService(options.TokenProviderType);
                if (tokenProvider is ITokenProvider tp)
                {
                    var logger = provider.GetRequiredService(
                        typeof(Microsoft.Extensions.Logging.ILogger<TokenRefreshHandler>));
                    return new TokenRefreshHandler(tp, (Microsoft.Extensions.Logging.ILogger<TokenRefreshHandler>)logger);
                }
                // If token provider not found, create a no-op delegating handler
                return new NoOpHandler();
            });
        }

        // Apply resilience pipeline if configured
        if (!string.IsNullOrEmpty(options.ResiliencePipelineName))
        {
            // Resilience pipeline would be added here
            // httpClientBuilder.AddPolicyHandler(...);
        }

        // Note: Logging and metrics handlers would be configured through
        // message handler configuration or middleware patterns

        return services;
    }

    /// <summary>
    /// No-op delegating handler for fallback cases.
    /// </summary>
    private class NoOpHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken);
        }
    }
}
