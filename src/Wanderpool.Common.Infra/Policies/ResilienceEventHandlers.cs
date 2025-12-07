using Microsoft.Extensions.Logging;

namespace Wanderpool.Common.Infra.Policies;

/// <summary>
/// Event handlers for Polly resilience policy events with structured logging.
/// Provides logging callbacks for retry, circuit breaker, timeout, and hedging events.
/// </summary>
public class ResilienceEventHandlers
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the ResilienceEventHandlers class.
    /// </summary>
    /// <param name="logger">Logger for recording resilience events.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public ResilienceEventHandlers(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Called when a retry attempt is about to be made.
    /// </summary>
    /// <param name="attemptNumber">The retry attempt number (1 = first retry, not the initial attempt).</param>
    /// <param name="delay">The delay before the next retry attempt.</param>
    /// <param name="exception">The exception that triggered the retry.</param>
    public void OnRetry(int attemptNumber, TimeSpan delay, Exception exception)
    {
        _logger.LogWarning(
            "Retry attempt {AttemptNumber} scheduled with delay {DelayMilliseconds}ms. " +
            "Exception: {ExceptionType} - {ExceptionMessage}",
            attemptNumber,
            delay.TotalMilliseconds,
            exception?.GetType().Name,
            exception?.Message);
    }

    /// <summary>
    /// Called when the circuit breaker opens (stops handling requests).
    /// </summary>
    public void OnCircuitBreakerOpened()
    {
        _logger.LogWarning(
            "Circuit breaker opened. Too many failures detected. " +
            "Requests will fail fast to prevent cascading failures.");
    }

    /// <summary>
    /// Called when the circuit breaker transitions to half-open state (testing if service recovered).
    /// </summary>
    public void OnCircuitBreakerHalfOpen()
    {
        _logger.LogInformation(
            "Circuit breaker transitioned to half-open state. " +
            "Testing if the service has recovered with a limited number of requests.");
    }

    /// <summary>
    /// Called when the circuit breaker closes (resumes normal operation).
    /// </summary>
    public void OnCircuitBreakerClosed()
    {
        _logger.LogInformation(
            "Circuit breaker closed. Service has recovered and is accepting requests again.");
    }

    /// <summary>
    /// Called when a request timeout occurs.
    /// </summary>
    /// <param name="timeoutDuration">The configured timeout duration.</param>
    public void OnTimeout(TimeSpan timeoutDuration)
    {
        _logger.LogWarning(
            "Request timeout occurred. Timeout duration: {TimeoutSeconds} seconds. " +
            "Request did not complete within the configured time limit.",
            timeoutDuration.TotalSeconds);
    }

    /// <summary>
    /// Called when a hedging attempt is made (duplicate request for faster response).
    /// </summary>
    /// <param name="attemptNumber">The hedging attempt number.</param>
    public void OnHedging(int attemptNumber)
    {
        _logger.LogInformation(
            "Hedging attempt {AttemptNumber} triggered. " +
            "Sending duplicate request to reduce P99 latency.",
            attemptNumber);
    }

    /// <summary>
    /// Gets a delegate for retry event handling suitable for Polly OnRetryAsync callback.
    /// </summary>
    /// <returns>A delegate that can be used as an event handler for retry events.</returns>
    public Action<int, TimeSpan, Exception> GetRetryHandler()
    {
        return OnRetry;
    }

    /// <summary>
    /// Gets a delegate for circuit breaker opened event handling.
    /// </summary>
    /// <returns>A delegate that can be used as an event handler for circuit breaker opened events.</returns>
    public Action GetCircuitBreakerOpenedHandler()
    {
        return OnCircuitBreakerOpened;
    }

    /// <summary>
    /// Gets a delegate for circuit breaker half-open event handling.
    /// </summary>
    /// <returns>A delegate that can be used as an event handler for circuit breaker half-open events.</returns>
    public Action GetCircuitBreakerHalfOpenHandler()
    {
        return OnCircuitBreakerHalfOpen;
    }

    /// <summary>
    /// Gets a delegate for circuit breaker closed event handling.
    /// </summary>
    /// <returns>A delegate that can be used as an event handler for circuit breaker closed events.</returns>
    public Action GetCircuitBreakerClosedHandler()
    {
        return OnCircuitBreakerClosed;
    }

    /// <summary>
    /// Gets a delegate for timeout event handling.
    /// </summary>
    /// <returns>A delegate that can be used as an event handler for timeout events.</returns>
    public Action<TimeSpan> GetTimeoutHandler()
    {
        return OnTimeout;
    }

    /// <summary>
    /// Gets a delegate for hedging event handling.
    /// </summary>
    /// <returns>A delegate that can be used as an event handler for hedging events.</returns>
    public Action<int> GetHedgingHandler()
    {
        return OnHedging;
    }
}
