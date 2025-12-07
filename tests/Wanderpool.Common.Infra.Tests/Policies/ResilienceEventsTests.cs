using Microsoft.Extensions.Logging;
using Xunit;
using Wanderpool.Common.Infra.Policies;

namespace Wanderpool.Common.Infra.Tests.Policies;

/// <summary>
/// Tests for resilience event handlers and logging.
/// Validates that Polly resilience events are properly logged.
/// </summary>
public class ResilienceEventsTests
{
    /// <summary>
    /// Mock logger for capturing logged events.
    /// </summary>
    private class MockLogger : ILogger
    {
        public List<(LogLevel Level, string Message, Exception? Exception)> LogEntries { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            LogEntries.Add((logLevel, formatter(state, exception), exception));
        }
    }

    /// <summary>
    /// Test: Retry event handler logs retry attempts.
    /// </summary>
    [Fact]
    public void ResilienceEventHandlers_OnRetry_LogsRetryAttempt()
    {
        // Arrange
        var logger = new MockLogger();
        var handler = new ResilienceEventHandlers(logger);
        var attemptNumber = 2;
        var delay = TimeSpan.FromMilliseconds(500);
        var exception = new HttpRequestException("Service unavailable");

        // Act
        handler.OnRetry(attemptNumber, delay, exception);

        // Assert
        Assert.NotEmpty(logger.LogEntries);
        var logEntry = logger.LogEntries.First();
        Assert.Equal(LogLevel.Warning, logEntry.Level);
        Assert.Contains("retry", logEntry.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("2", logEntry.Message); // attempt number
    }

    /// <summary>
    /// Test: Circuit breaker opened event is logged.
    /// </summary>
    [Fact]
    public void ResilienceEventHandlers_OnCircuitBreakerOpened_LogsCircuitOpened()
    {
        // Arrange
        var logger = new MockLogger();
        var handler = new ResilienceEventHandlers(logger);

        // Act
        handler.OnCircuitBreakerOpened();

        // Assert
        Assert.NotEmpty(logger.LogEntries);
        var logEntry = logger.LogEntries.First();
        Assert.Equal(LogLevel.Warning, logEntry.Level);
        Assert.Contains("circuit breaker", logEntry.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("open", logEntry.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Test: Circuit breaker half-open event is logged.
    /// </summary>
    [Fact]
    public void ResilienceEventHandlers_OnCircuitBreakerHalfOpen_LogsCircuitHalfOpen()
    {
        // Arrange
        var logger = new MockLogger();
        var handler = new ResilienceEventHandlers(logger);

        // Act
        handler.OnCircuitBreakerHalfOpen();

        // Assert
        Assert.NotEmpty(logger.LogEntries);
        var logEntry = logger.LogEntries.First();
        Assert.Equal(LogLevel.Information, logEntry.Level);
        Assert.Contains("circuit breaker", logEntry.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("half-open", logEntry.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Test: Circuit breaker closed event is logged.
    /// </summary>
    [Fact]
    public void ResilienceEventHandlers_OnCircuitBreakerClosed_LogsCircuitClosed()
    {
        // Arrange
        var logger = new MockLogger();
        var handler = new ResilienceEventHandlers(logger);

        // Act
        handler.OnCircuitBreakerClosed();

        // Assert
        Assert.NotEmpty(logger.LogEntries);
        var logEntry = logger.LogEntries.First();
        Assert.Equal(LogLevel.Information, logEntry.Level);
        Assert.Contains("circuit breaker", logEntry.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("closed", logEntry.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Test: Timeout event is logged with request details.
    /// </summary>
    [Fact]
    public void ResilienceEventHandlers_OnTimeout_LogsTimeoutEvent()
    {
        // Arrange
        var logger = new MockLogger();
        var handler = new ResilienceEventHandlers(logger);
        var timeoutDuration = TimeSpan.FromSeconds(10);

        // Act
        handler.OnTimeout(timeoutDuration);

        // Assert
        Assert.NotEmpty(logger.LogEntries);
        var logEntry = logger.LogEntries.First();
        Assert.Equal(LogLevel.Warning, logEntry.Level);
        Assert.Contains("timeout", logEntry.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("10", logEntry.Message); // timeout seconds
    }

    /// <summary>
    /// Test: Hedging event is logged with attempt details.
    /// </summary>
    [Fact]
    public void ResilienceEventHandlers_OnHedging_LogsHedgingAttempt()
    {
        // Arrange
        var logger = new MockLogger();
        var handler = new ResilienceEventHandlers(logger);
        var attemptNumber = 1;

        // Act
        handler.OnHedging(attemptNumber);

        // Assert
        Assert.NotEmpty(logger.LogEntries);
        var logEntry = logger.LogEntries.First();
        Assert.Equal(LogLevel.Information, logEntry.Level);
        Assert.Contains("hedging", logEntry.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("1", logEntry.Message); // attempt number
    }

    /// <summary>
    /// Test: Event handlers can be instantiated and used.
    /// </summary>
    [Fact]
    public void ResilienceEventHandlers_CanBeInstantiatedAndUsed()
    {
        // Arrange
        var logger = new MockLogger();

        // Act
        var handler = new ResilienceEventHandlers(logger);

        // Assert
        Assert.NotNull(handler);
    }

    /// <summary>
    /// Test: Null logger throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void ResilienceEventHandlers_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ResilienceEventHandlers(null!));
    }
}
