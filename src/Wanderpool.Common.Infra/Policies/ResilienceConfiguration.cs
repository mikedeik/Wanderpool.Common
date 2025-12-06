namespace Wanderpool.Common.Infra.Policies;

/// <summary>
/// Configuration for HTTP resilience policies (retry, timeout, circuit breaker, hedging).
/// Maps to appsettings.json "Resilience" section.
/// </summary>
public class ResilienceConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string Name = "Resilience";

    /// <summary>
    /// Timeout configuration for individual HTTP requests.
    /// </summary>
    public TimeoutConfig Timeout { get; set; } = new();

    /// <summary>
    /// Retry configuration with exponential backoff.
    /// </summary>
    public RetryConfig Retry { get; set; } = new();

    /// <summary>
    /// Circuit breaker configuration.
    /// </summary>
    public CircuitBreakerConfig CircuitBreaker { get; set; } = new();

    /// <summary>
    /// Hedging configuration for improved P99 latency.
    /// </summary>
    public HedgingConfig Hedging { get; set; } = new();

    /// <summary>
    /// Timeout policy configuration.
    /// </summary>
    public class TimeoutConfig
    {
        /// <summary>
        /// Timeout per HTTP request attempt in seconds.
        /// Default: 10 seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 10;
    }

    /// <summary>
    /// Retry policy configuration.
    /// </summary>
    public class RetryConfig
    {
        /// <summary>
        /// Maximum number of retry attempts.
        /// Default: 3
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Initial delay between retries in milliseconds.
        /// Used with exponential backoff calculation.
        /// Default: 300 ms
        /// </summary>
        public int InitialDelayMilliseconds { get; set; } = 300;

        /// <summary>
        /// Whether to use jitter in retry delays to avoid thundering herd.
        /// Default: true
        /// </summary>
        public bool UseJitter { get; set; } = true;
    }

    /// <summary>
    /// Circuit breaker policy configuration.
    /// Prevents cascading failures by failing fast when service is unhealthy.
    /// </summary>
    public class CircuitBreakerConfig
    {
        /// <summary>
        /// Duration the circuit breaker remains open after opening (in seconds).
        /// Default: 20 seconds
        /// </summary>
        public int BreakDurationSeconds { get; set; } = 20;

        /// <summary>
        /// Duration over which to sample requests for health evaluation (in seconds).
        /// Default: 30 seconds
        /// </summary>
        public int SamplingDurationSeconds { get; set; } = 30;

        /// <summary>
        /// Failure ratio threshold (0.0 to 1.0) that triggers circuit opening.
        /// Opens if this percentage of requests fail.
        /// Default: 0.25 (25%)
        /// </summary>
        public double FailureRatio { get; set; } = 0.25;

        /// <summary>
        /// Minimum number of requests in sampling window before evaluation.
        /// Circuit breaker only evaluates after this many requests.
        /// Default: 20
        /// </summary>
        public int MinimumThroughput { get; set; } = 20;
    }

    /// <summary>
    /// Hedging policy configuration.
    /// Sends duplicate requests after a delay to improve P99 latency.
    /// </summary>
    public class HedgingConfig
    {
        /// <summary>
        /// Delay before sending a hedged request in milliseconds.
        /// Default: 200 ms
        /// </summary>
        public int DelayMilliseconds { get; set; } = 200;

        /// <summary>
        /// Maximum number of hedged attempts (additional requests beyond the first).
        /// Default: 2 (up to 3 total requests: 1 original + 2 hedged)
        /// </summary>
        public int MaxHedgedAttempts { get; set; } = 2;
    }
}
