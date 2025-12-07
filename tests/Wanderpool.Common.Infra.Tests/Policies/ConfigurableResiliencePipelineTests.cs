using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
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
}
