using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Timeout;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Wanderpool.Common.Infra.Policies;

public static class ResiliencePipelines
{
    /// <summary>
    /// Adds HTTP resilience policies with hardcoded defaults.
    /// Use AddStandardResilienceWithConfiguration() for environment-specific settings.
    /// </summary>
    /// <remarks>
    /// Deprecated: Use AddStandardResilienceWithConfiguration with IOptions pattern instead.
    /// </remarks>
    public static void AddStandardResilience(this IHttpClientBuilder clientBuilder)
    {
        var config = new ResilienceConfiguration();
        AddStandardResilienceInternal(clientBuilder, config);
    }

    /// <summary>
    /// Adds HTTP resilience policies with configuration from appsettings.json.
    /// </summary>
    /// <param name="clientBuilder">The HTTP client builder.</param>
    /// <param name="options">The resilience configuration options.</param>
    public static void AddStandardResilienceWithConfiguration(
        this IHttpClientBuilder clientBuilder,
        IOptions<ResilienceConfiguration> options)
    {
        AddStandardResilienceInternal(clientBuilder, options.Value);
    }

    /// <summary>
    /// Internal implementation of resilience policies.
    /// </summary>
    private static void AddStandardResilienceInternal(
        IHttpClientBuilder clientBuilder,
        ResilienceConfiguration config)
    {
        clientBuilder.AddResilienceHandler("standard", builder =>
        {
            // Timeout per try
            builder.AddTimeout(new HttpTimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(config.Timeout.TimeoutSeconds),
            });

            // Retry with exponential backoff + jitter
            builder.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = config.Retry.MaxRetryAttempts,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromMilliseconds(config.Retry.InitialDelayMilliseconds),
                UseJitter = config.Retry.UseJitter
            });

            // Circuit breaker
            builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                BreakDuration = TimeSpan.FromSeconds(config.CircuitBreaker.BreakDurationSeconds),
                SamplingDuration = TimeSpan.FromSeconds(config.CircuitBreaker.SamplingDurationSeconds),
                FailureRatio = config.CircuitBreaker.FailureRatio,
                MinimumThroughput = config.CircuitBreaker.MinimumThroughput
            });

            // Hedging for better P99 latency
            builder.AddHedging(new HttpHedgingStrategyOptions
            {
                Delay = TimeSpan.FromMilliseconds(config.Hedging.DelayMilliseconds),
                MaxHedgedAttempts = config.Hedging.MaxHedgedAttempts
            });
        });
    }
}
