using Xunit;
using Wanderpool.Common.Infra.Policies;

namespace Wanderpool.Common.Infra.Tests.Policies;

/// <summary>
/// Tests for ResilienceOptions configuration class.
/// Validates default values and configuration for resilience policies.
/// </summary>
public class ResilienceOptionsTests
{
    /// <summary>
    /// Test: ResilienceOptions has correct configuration section name.
    /// </summary>
    [Fact]
    public void ResilienceOptions_HasCorrectConfigurationName()
    {
        // Assert
        Assert.Equal("Resilience", ResilienceOptions.Name);
    }

    /// <summary>
    /// Test: TimeoutPolicyOptions has sensible default timeout value.
    /// </summary>
    [Fact]
    public void TimeoutPolicyOptions_HasDefaultTimeout()
    {
        // Arrange & Act
        var options = new ResilienceOptions();

        // Assert
        Assert.NotNull(options.Timeout);
        Assert.Equal(10, options.Timeout.TimeoutSeconds);
        Assert.Equal("Timeout", options.Timeout.PolicyName);
    }

    /// <summary>
    /// Test: TimeoutPolicyOptions timeout can be configured.
    /// </summary>
    [Fact]
    public void TimeoutPolicyOptions_CanBeConfigured()
    {
        // Arrange
        var options = new ResilienceOptions();

        // Act
        options.Timeout.TimeoutSeconds = 30;

        // Assert
        Assert.Equal(30, options.Timeout.TimeoutSeconds);
    }

    /// <summary>
    /// Test: RetryPolicyOptions has sensible default retry values.
    /// </summary>
    [Fact]
    public void RetryPolicyOptions_HasDefaultRetryValues()
    {
        // Arrange & Act
        var options = new ResilienceOptions();

        // Assert
        Assert.NotNull(options.Retry);
        Assert.Equal(3, options.Retry.MaxRetryAttempts);
        Assert.Equal(300, options.Retry.InitialDelayMilliseconds);
        Assert.Equal(30000, options.Retry.MaxDelayMilliseconds);
        Assert.True(options.Retry.UseExponentialBackoff);
        Assert.Equal("Retry", options.Retry.PolicyName);
    }

    /// <summary>
    /// Test: RetryPolicyOptions can be fully configured.
    /// </summary>
    [Fact]
    public void RetryPolicyOptions_CanBeFullyConfigured()
    {
        // Arrange
        var options = new ResilienceOptions();

        // Act
        options.Retry.MaxRetryAttempts = 5;
        options.Retry.InitialDelayMilliseconds = 500;
        options.Retry.MaxDelayMilliseconds = 60000;
        options.Retry.UseExponentialBackoff = false;

        // Assert
        Assert.Equal(5, options.Retry.MaxRetryAttempts);
        Assert.Equal(500, options.Retry.InitialDelayMilliseconds);
        Assert.Equal(60000, options.Retry.MaxDelayMilliseconds);
        Assert.False(options.Retry.UseExponentialBackoff);
    }

    /// <summary>
    /// Test: CircuitBreakerPolicyOptions has sensible default values.
    /// </summary>
    [Fact]
    public void CircuitBreakerPolicyOptions_HasDefaultValues()
    {
        // Arrange & Act
        var options = new ResilienceOptions();

        // Assert
        Assert.NotNull(options.CircuitBreaker);
        Assert.Equal(0.25, options.CircuitBreaker.FailureRatio);
        Assert.Equal(20, options.CircuitBreaker.MinimumThroughput);
        Assert.Equal(30, options.CircuitBreaker.SamplingPeriodSeconds);
        Assert.Equal(20, options.CircuitBreaker.BreakDurationSeconds);
        Assert.Equal("CircuitBreaker", options.CircuitBreaker.PolicyName);
    }

    /// <summary>
    /// Test: CircuitBreakerPolicyOptions can be fully configured.
    /// </summary>
    [Fact]
    public void CircuitBreakerPolicyOptions_CanBeFullyConfigured()
    {
        // Arrange
        var options = new ResilienceOptions();

        // Act
        options.CircuitBreaker.FailureRatio = 0.5;
        options.CircuitBreaker.MinimumThroughput = 10;
        options.CircuitBreaker.SamplingPeriodSeconds = 60;
        options.CircuitBreaker.BreakDurationSeconds = 30;

        // Assert
        Assert.Equal(0.5, options.CircuitBreaker.FailureRatio);
        Assert.Equal(10, options.CircuitBreaker.MinimumThroughput);
        Assert.Equal(60, options.CircuitBreaker.SamplingPeriodSeconds);
        Assert.Equal(30, options.CircuitBreaker.BreakDurationSeconds);
    }

    /// <summary>
    /// Test: HedgingStrategyOptions has sensible default values with hedging disabled.
    /// </summary>
    [Fact]
    public void HedgingStrategyOptions_HasDefaultValuesDisabled()
    {
        // Arrange & Act
        var options = new ResilienceOptions();

        // Assert
        Assert.NotNull(options.Hedging);
        Assert.Equal(200, options.Hedging.DelayMilliseconds);
        Assert.Equal(2, options.Hedging.MaxHedgedAttempts);
        Assert.False(options.Hedging.Enabled);
        Assert.Equal("Hedging", options.Hedging.StrategyName);
    }

    /// <summary>
    /// Test: HedgingStrategyOptions can be configured and enabled.
    /// </summary>
    [Fact]
    public void HedgingStrategyOptions_CanBeConfiguredAndEnabled()
    {
        // Arrange
        var options = new ResilienceOptions();

        // Act
        options.Hedging.DelayMilliseconds = 100;
        options.Hedging.MaxHedgedAttempts = 3;
        options.Hedging.Enabled = true;

        // Assert
        Assert.Equal(100, options.Hedging.DelayMilliseconds);
        Assert.Equal(3, options.Hedging.MaxHedgedAttempts);
        Assert.True(options.Hedging.Enabled);
    }

    /// <summary>
    /// Test: All nested policy option classes are initialized on ResilienceOptions creation.
    /// </summary>
    [Fact]
    public void ResilienceOptions_InitializesAllPolicies()
    {
        // Act
        var options = new ResilienceOptions();

        // Assert - verify all are not null and properly initialized
        Assert.NotNull(options.Timeout);
        Assert.NotNull(options.Retry);
        Assert.NotNull(options.CircuitBreaker);
        Assert.NotNull(options.Hedging);

        // Verify they have default values
        Assert.NotEqual(0, options.Timeout.TimeoutSeconds);
        Assert.NotEqual(0, options.Retry.MaxRetryAttempts);
        Assert.NotEqual(0, options.CircuitBreaker.MinimumThroughput);
        Assert.NotEqual(0, options.Hedging.DelayMilliseconds);
    }

    /// <summary>
    /// Test: ResilienceOptions can be created and all policies configured independently.
    /// </summary>
    [Fact]
    public void ResilienceOptions_AllPoliciesCanBeConfiguredIndependently()
    {
        // Act
        var options = new ResilienceOptions
        {
            Timeout = new ResilienceOptions.TimeoutPolicyOptions { TimeoutSeconds = 20 },
            Retry = new ResilienceOptions.RetryPolicyOptions { MaxRetryAttempts = 4 },
            CircuitBreaker = new ResilienceOptions.CircuitBreakerPolicyOptions { FailureRatio = 0.5 },
            Hedging = new ResilienceOptions.HedgingStrategyOptions { Enabled = true }
        };

        // Assert
        Assert.Equal(20, options.Timeout.TimeoutSeconds);
        Assert.Equal(4, options.Retry.MaxRetryAttempts);
        Assert.Equal(0.5, options.CircuitBreaker.FailureRatio);
        Assert.True(options.Hedging.Enabled);
    }
}
