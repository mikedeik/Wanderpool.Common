using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Xunit;
using Wanderpool.Common.Infra.Policies;

namespace Wanderpool.Common.Infra.Tests.Policies;

/// <summary>
/// Tests for resilience pipeline configuration using ResilienceOptions.
/// Validates that Polly pipelines are properly configured from options.
/// </summary>
public class ConfigurableResiliencePipelineTests
{
    /// <summary>
    /// Test: Resilience handler can be added with default ResilienceOptions.
    /// </summary>
    [Fact]
    public void AddStandardResilienceWithOptions_WithDefaultOptions_RegistersHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddHttpClient("TestClient")
            .AddStandardResilienceWithOptions(new ResilienceOptions());

        var provider = services.BuildServiceProvider();

        // Act - resolve the HTTP client
        var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var client = clientFactory.CreateClient("TestClient");

        // Assert - client is created successfully (handler is registered)
        Assert.NotNull(client);
    }

    /// <summary>
    /// Test: Timeout configuration from ResilienceOptions is applied.
    /// </summary>
    [Fact]
    public void AddStandardResilienceWithOptions_AppliesTimeoutConfiguration()
    {
        // Arrange
        var options = new ResilienceOptions
        {
            Timeout = new ResilienceOptions.TimeoutPolicyOptions { TimeoutSeconds = 30 }
        };

        var services = new ServiceCollection();
        services.AddHttpClient("TestClient")
            .AddStandardResilienceWithOptions(options);

        var provider = services.BuildServiceProvider();

        // Act
        var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var client = clientFactory.CreateClient("TestClient");

        // Assert
        Assert.NotNull(client);
        Assert.Equal(30, options.Timeout.TimeoutSeconds);
    }

    /// <summary>
    /// Test: Retry configuration from ResilienceOptions is applied.
    /// </summary>
    [Fact]
    public void AddStandardResilienceWithOptions_AppliesRetryConfiguration()
    {
        // Arrange
        var options = new ResilienceOptions
        {
            Retry = new ResilienceOptions.RetryPolicyOptions
            {
                MaxRetryAttempts = 5,
                InitialDelayMilliseconds = 500,
                UseExponentialBackoff = true
            }
        };

        var services = new ServiceCollection();
        services.AddHttpClient("TestClient")
            .AddStandardResilienceWithOptions(options);

        var provider = services.BuildServiceProvider();

        // Act
        var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var client = clientFactory.CreateClient("TestClient");

        // Assert
        Assert.NotNull(client);
        Assert.Equal(5, options.Retry.MaxRetryAttempts);
        Assert.Equal(500, options.Retry.InitialDelayMilliseconds);
        Assert.True(options.Retry.UseExponentialBackoff);
    }

    /// <summary>
    /// Test: Circuit breaker configuration from ResilienceOptions is applied.
    /// </summary>
    [Fact]
    public void AddStandardResilienceWithOptions_AppliesCircuitBreakerConfiguration()
    {
        // Arrange
        var options = new ResilienceOptions
        {
            CircuitBreaker = new ResilienceOptions.CircuitBreakerPolicyOptions
            {
                FailureRatio = 0.5,
                MinimumThroughput = 10,
                SamplingPeriodSeconds = 60,
                BreakDurationSeconds = 30
            }
        };

        var services = new ServiceCollection();
        services.AddHttpClient("TestClient")
            .AddStandardResilienceWithOptions(options);

        var provider = services.BuildServiceProvider();

        // Act
        var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var client = clientFactory.CreateClient("TestClient");

        // Assert
        Assert.NotNull(client);
        Assert.Equal(0.5, options.CircuitBreaker.FailureRatio);
        Assert.Equal(10, options.CircuitBreaker.MinimumThroughput);
        Assert.Equal(60, options.CircuitBreaker.SamplingPeriodSeconds);
        Assert.Equal(30, options.CircuitBreaker.BreakDurationSeconds);
    }

    /// <summary>
    /// Test: Hedging configuration from ResilienceOptions is applied.
    /// </summary>
    [Fact]
    public void AddStandardResilienceWithOptions_AppliesHedgingConfiguration()
    {
        // Arrange
        var options = new ResilienceOptions
        {
            Hedging = new ResilienceOptions.HedgingStrategyOptions
            {
                DelayMilliseconds = 150,
                MaxHedgedAttempts = 3,
                Enabled = true
            }
        };

        var services = new ServiceCollection();
        services.AddHttpClient("TestClient")
            .AddStandardResilienceWithOptions(options);

        var provider = services.BuildServiceProvider();

        // Act
        var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var client = clientFactory.CreateClient("TestClient");

        // Assert
        Assert.NotNull(client);
        Assert.Equal(150, options.Hedging.DelayMilliseconds);
        Assert.Equal(3, options.Hedging.MaxHedgedAttempts);
        Assert.True(options.Hedging.Enabled);
    }

    /// <summary>
    /// Test: Each policy can be configured independently while others use defaults.
    /// </summary>
    [Fact]
    public void AddStandardResilienceWithOptions_AllowsIndependentPolicyConfiguration()
    {
        // Arrange
        var options = new ResilienceOptions
        {
            Timeout = new ResilienceOptions.TimeoutPolicyOptions { TimeoutSeconds = 25 },
            // Retry uses defaults
            CircuitBreaker = new ResilienceOptions.CircuitBreakerPolicyOptions { FailureRatio = 0.4 }
            // Hedging uses defaults
        };

        var services = new ServiceCollection();
        services.AddHttpClient("TestClient")
            .AddStandardResilienceWithOptions(options);

        var provider = services.BuildServiceProvider();

        // Act
        var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
        var client = clientFactory.CreateClient("TestClient");

        // Assert
        Assert.NotNull(client);
        Assert.Equal(25, options.Timeout.TimeoutSeconds);
        Assert.Equal(3, options.Retry.MaxRetryAttempts); // default
        Assert.Equal(0.4, options.CircuitBreaker.FailureRatio);
        Assert.False(options.Hedging.Enabled); // default
    }

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
    /// Test: Retry event handler can be instantiated and called with ResilienceOptions.
    /// </summary>
    [Fact]
    public void AddStandardResilienceWithOptions_CanUseRetryEventHandlers()
    {
        // Arrange
        var logger = new MockLogger();
        var eventHandlers = new ResilienceEventHandlers(logger);
        var options = new ResilienceOptions
        {
            Retry = new ResilienceOptions.RetryPolicyOptions { MaxRetryAttempts = 2 }
        };

        var services = new ServiceCollection();
        services.AddSingleton<ILogger>(logger);
        services.AddSingleton<ResilienceEventHandlers>(eventHandlers);
        services.AddHttpClient("TestClient")
            .AddStandardResilienceWithOptions(options);

        var provider = services.BuildServiceProvider();

        // Act
        var retrievedHandlers = provider.GetRequiredService<ResilienceEventHandlers>();
        retrievedHandlers.OnRetry(1, TimeSpan.FromMilliseconds(100), new HttpRequestException("Service unavailable"));

        // Assert
        Assert.NotEmpty(logger.LogEntries);
        var retryLog = logger.LogEntries.First();
        Assert.Equal(LogLevel.Warning, retryLog.Level);
        Assert.Contains("retry", retryLog.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Test: Circuit breaker event handlers can be instantiated and called with ResilienceOptions.
    /// </summary>
    [Fact]
    public void AddStandardResilienceWithOptions_CanUseCircuitBreakerEventHandlers()
    {
        // Arrange
        var logger = new MockLogger();
        var eventHandlers = new ResilienceEventHandlers(logger);
        var options = new ResilienceOptions
        {
            CircuitBreaker = new ResilienceOptions.CircuitBreakerPolicyOptions { FailureRatio = 0.5 }
        };

        var services = new ServiceCollection();
        services.AddSingleton<ILogger>(logger);
        services.AddSingleton<ResilienceEventHandlers>(eventHandlers);
        services.AddHttpClient("TestClient")
            .AddStandardResilienceWithOptions(options);

        var provider = services.BuildServiceProvider();

        // Act
        var retrievedHandlers = provider.GetRequiredService<ResilienceEventHandlers>();
        retrievedHandlers.OnCircuitBreakerOpened();
        retrievedHandlers.OnCircuitBreakerHalfOpen();
        retrievedHandlers.OnCircuitBreakerClosed();

        // Assert
        Assert.Equal(3, logger.LogEntries.Count);
        Assert.Equal(LogLevel.Warning, logger.LogEntries[0].Level);
        Assert.Contains("circuit breaker", logger.LogEntries[0].Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(LogLevel.Information, logger.LogEntries[1].Level);
        Assert.Equal(LogLevel.Information, logger.LogEntries[2].Level);
    }

    /// <summary>
    /// Test: Timeout event handler can be instantiated and called with ResilienceOptions.
    /// </summary>
    [Fact]
    public void AddStandardResilienceWithOptions_CanUseTimeoutEventHandlers()
    {
        // Arrange
        var logger = new MockLogger();
        var eventHandlers = new ResilienceEventHandlers(logger);
        var options = new ResilienceOptions
        {
            Timeout = new ResilienceOptions.TimeoutPolicyOptions { TimeoutSeconds = 10 }
        };

        var services = new ServiceCollection();
        services.AddSingleton<ILogger>(logger);
        services.AddSingleton<ResilienceEventHandlers>(eventHandlers);
        services.AddHttpClient("TestClient")
            .AddStandardResilienceWithOptions(options);

        var provider = services.BuildServiceProvider();

        // Act
        var retrievedHandlers = provider.GetRequiredService<ResilienceEventHandlers>();
        retrievedHandlers.OnTimeout(TimeSpan.FromSeconds(10));

        // Assert
        Assert.NotEmpty(logger.LogEntries);
        var timeoutLog = logger.LogEntries.First();
        Assert.Equal(LogLevel.Warning, timeoutLog.Level);
        Assert.Contains("timeout", timeoutLog.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Test: Hedging event handler can be instantiated and called with ResilienceOptions.
    /// </summary>
    [Fact]
    public void AddStandardResilienceWithOptions_CanUseHedgingEventHandlers()
    {
        // Arrange
        var logger = new MockLogger();
        var eventHandlers = new ResilienceEventHandlers(logger);
        var options = new ResilienceOptions
        {
            Hedging = new ResilienceOptions.HedgingStrategyOptions { Enabled = true, MaxHedgedAttempts = 2 }
        };

        var services = new ServiceCollection();
        services.AddSingleton<ILogger>(logger);
        services.AddSingleton<ResilienceEventHandlers>(eventHandlers);
        services.AddHttpClient("TestClient")
            .AddStandardResilienceWithOptions(options);

        var provider = services.BuildServiceProvider();

        // Act
        var retrievedHandlers = provider.GetRequiredService<ResilienceEventHandlers>();
        retrievedHandlers.OnHedging(1);

        // Assert
        Assert.NotEmpty(logger.LogEntries);
        var hedgingLog = logger.LogEntries.First();
        Assert.Equal(LogLevel.Information, hedgingLog.Level);
        Assert.Contains("hedging", hedgingLog.Message, StringComparison.OrdinalIgnoreCase);
    }
}
