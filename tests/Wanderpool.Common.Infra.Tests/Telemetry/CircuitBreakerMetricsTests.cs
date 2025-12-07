using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Wanderpool.Common.Infra.Telemetry;

namespace Wanderpool.Common.Infra.Tests.Telemetry;

/// <summary>
/// Tests for circuit breaker state metrics instrumentation.
/// </summary>
public class CircuitBreakerMetricsTests
{
    [Fact]
    public void AddWanderpoolCircuitBreakerMetrics_RegistersObservableGauge()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddWanderpoolMetrics();

        // Act
        services.AddWanderpoolCircuitBreakerMetrics();
        var provider = services.BuildServiceProvider();

        // Assert
        var instruments = provider.GetRequiredService<CircuitBreakerMetricsInstruments>();
        Assert.NotNull(instruments);
        Assert.NotNull(instruments.CircuitBreakerStateGauge);
    }

    [Fact]
    public void AddWanderpoolCircuitBreakerMetrics_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddWanderpoolMetrics();

        // Act
        var result = services.AddWanderpoolCircuitBreakerMetrics();

        // Assert
        Assert.Equal(services, result);
    }

    [Fact]
    public void CircuitBreakerMetricsInstruments_ContainsStateGauge()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddWanderpoolMetrics();
        services.AddWanderpoolCircuitBreakerMetrics();

        // Act
        var provider = services.BuildServiceProvider();
        var instruments = provider.GetRequiredService<CircuitBreakerMetricsInstruments>();

        // Assert
        Assert.NotNull(instruments.CircuitBreakerStateGauge);
    }

    [Fact]
    public void CircuitBreakerMetricsInstruments_ContainsStateRegistry()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddWanderpoolMetrics();
        services.AddWanderpoolCircuitBreakerMetrics();

        // Act
        var provider = services.BuildServiceProvider();
        var instruments = provider.GetRequiredService<CircuitBreakerMetricsInstruments>();

        // Assert
        Assert.NotNull(instruments.CircuitBreakerStateRegistry);
    }

    [Fact]
    public void CircuitBreakerStateRegistry_CanTrackCircuitBreakerState()
    {
        // Arrange
        var registry = new CircuitBreakerStateRegistry();

        // Act
        registry.SetState("test_circuit", 0); // Closed
        registry.SetState("test_circuit", 1); // Open
        registry.SetState("test_circuit", 2); // Half-Open

        // Assert
        Assert.Equal(2, registry.GetState("test_circuit"));
    }

    [Fact]
    public void CircuitBreakerStateRegistry_ReturnsZeroForUnknownCircuit()
    {
        // Arrange
        var registry = new CircuitBreakerStateRegistry();

        // Act
        var state = registry.GetState("unknown_circuit");

        // Assert
        Assert.Equal(0, state); // Default to Closed (0)
    }
}
