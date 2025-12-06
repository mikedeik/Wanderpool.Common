using Polly;

namespace Wanderpool.Common.Infra.Policies;

using Microsoft.Extensions.Http.Resilience;

public static class ResiliencePipelines
{
    public static void AddStandardResilience(this IHttpClientBuilder clientBuilder)
    {
        clientBuilder.AddResilienceHandler("standard", builder =>
        {
            // Timeout per try
            builder.AddTimeout(new HttpTimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(10),
            });

            // Retry with exponential backoff + jitter
            builder.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromMilliseconds(300),
                UseJitter = true
            });

            // Circuit breaker
            builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                BreakDuration = TimeSpan.FromSeconds(20),
                SamplingDuration = TimeSpan.FromSeconds(30),
                FailureRatio = 0.25,      // opens if 25% of requests fail
                MinimumThroughput = 20    // evaluates only after 20 requests          
            });

            // Hedging for better P99 latency
            builder.AddHedging(new HttpHedgingStrategyOptions
            {
                Delay = TimeSpan.FromMilliseconds(200),
                MaxHedgedAttempts = 2
            });
        });
    }
}
