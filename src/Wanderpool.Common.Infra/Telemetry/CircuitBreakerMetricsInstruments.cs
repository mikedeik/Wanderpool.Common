using System.Diagnostics.Metrics;

namespace Wanderpool.Common.Infra.Telemetry;

/// <summary>
/// Contains custom metric instruments for circuit breaker state tracking.
/// </summary>
public sealed class CircuitBreakerMetricsInstruments
{
    /// <summary>
    /// Observable gauge for tracking circuit breaker state.
    /// Values: 0 = Closed, 1 = Open, 2 = Half-Open
    /// Tagged by: circuit_name
    /// </summary>
    public required ObservableGauge<int> CircuitBreakerStateGauge { get; init; }

    /// <summary>
    /// Registry for managing circuit breaker states.
    /// Used by observables to report current state.
    /// </summary>
    public required CircuitBreakerStateRegistry CircuitBreakerStateRegistry { get; init; }
}
