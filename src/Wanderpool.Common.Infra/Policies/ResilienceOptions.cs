namespace Wanderpool.Common.Infra.Policies;

/// <summary>
/// Configuration options for resilience pipelines.
/// Maps to appsettings.json "Resilience" section.
/// </summary>
public class ResilienceOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string Name = "Resilience";

    /// <summary>
    /// Timeout policy configuration.
    /// </summary>
    public TimeoutPolicyOptions Timeout { get; set; } = new();

    /// <summary>
    /// Retry policy configuration.
    /// </summary>
    public RetryPolicyOptions Retry { get; set; } = new();

    /// <summary>
    /// Circuit breaker policy configuration.
    /// </summary>
    public CircuitBreakerPolicyOptions CircuitBreaker { get; set; } = new();

    /// <summary>
    /// Hedging strategy configuration.
    /// </summary>
    public HedgingStrategyOptions Hedging { get; set; } = new();

    /// <summary>
    /// Configuration for timeout policy.
    /// </summary>
    public class TimeoutPolicyOptions
    {
        /// <summary>
        /// Maximum time in seconds to wait for a request to complete.
        /// Default: 10 seconds.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 10;

        /// <summary>
        /// Timeout policy name.
        /// </summary>
        public string PolicyName { get; set; } = "Timeout";
    }

    /// <summary>
    /// Configuration for retry policy.
    /// Uses exponential backoff with jitter.
    /// </summary>
    public class RetryPolicyOptions
    {
        /// <summary>
        /// Maximum number of retry attempts (not including initial attempt).
        /// Default: 3 retries.
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Initial delay in milliseconds before first retry.
        /// Default: 300ms.
        /// </summary>
        public int InitialDelayMilliseconds { get; set; } = 300;

        /// <summary>
        /// Maximum delay in milliseconds between retries.
        /// Default: 30000ms (30 seconds).
        /// </summary>
        public int MaxDelayMilliseconds { get; set; } = 30000;

        /// <summary>
        /// Whether to use exponential backoff.
        /// Default: true.
        /// </summary>
        public bool UseExponentialBackoff { get; set; } = true;

        /// <summary>
        /// Retry policy name.
        /// </summary>
        public string PolicyName { get; set; } = "Retry";
    }

    /// <summary>
    /// Configuration for circuit breaker policy.
    /// Prevents cascading failures by stopping requests when failure rate exceeds threshold.
    /// </summary>
    public class CircuitBreakerPolicyOptions
    {
        /// <summary>
        /// Failure ratio (0.0 to 1.0) that triggers circuit opening.
        /// Default: 0.25 (25% failure rate).
        /// </summary>
        public double FailureRatio { get; set; } = 0.25;

        /// <summary>
        /// Minimum number of requests in sample period before triggering circuit breaker.
        /// Default: 20.
        /// </summary>
        public int MinimumThroughput { get; set; } = 20;

        /// <summary>
        /// Sample period in seconds for calculating failure ratio.
        /// Default: 30 seconds.
        /// </summary>
        public int SamplingPeriodSeconds { get; set; } = 30;

        /// <summary>
        /// Duration in seconds to hold circuit in open state before allowing test requests.
        /// Default: 20 seconds.
        /// </summary>
        public int BreakDurationSeconds { get; set; } = 20;

        /// <summary>
        /// Circuit breaker policy name.
        /// </summary>
        public string PolicyName { get; set; } = "CircuitBreaker";
    }

    /// <summary>
    /// Configuration for hedging strategy.
    /// Sends duplicate requests after delay to improve P99 latency.
    /// </summary>
    public class HedgingStrategyOptions
    {
        /// <summary>
        /// Delay in milliseconds before sending hedging request.
        /// Default: 200ms.
        /// </summary>
        public int DelayMilliseconds { get; set; } = 200;

        /// <summary>
        /// Maximum number of hedged attempts (not including primary).
        /// Default: 2.
        /// </summary>
        public int MaxHedgedAttempts { get; set; } = 2;

        /// <summary>
        /// Whether hedging is enabled.
        /// Default: false (disabled by default - can degrade performance if misconfigured).
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Hedging strategy name.
        /// </summary>
        public string StrategyName { get; set; } = "Hedging";
    }
}
