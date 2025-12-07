using System.Collections.Concurrent;

namespace Wanderpool.Common.Infra.Telemetry;

/// <summary>
/// Thread-safe registry for tracking circuit breaker states.
/// Used for observable metrics to report current state of circuit breakers.
/// </summary>
public sealed class CircuitBreakerStateRegistry
{
    private readonly ConcurrentDictionary<string, int> _states = new();

    /// <summary>
    /// Circuit breaker state enumeration.
    /// 0 = Closed (normal operation)
    /// 1 = Open (rejecting requests)
    /// 2 = Half-Open (testing recovery)
    /// </summary>
    public const int ClosedState = 0;
    public const int OpenState = 1;
    public const int HalfOpenState = 2;

    /// <summary>
    /// Sets the state of a circuit breaker.
    /// </summary>
    /// <param name="circuitName">The name of the circuit breaker.</param>
    /// <param name="state">The state value (0=Closed, 1=Open, 2=Half-Open).</param>
    public void SetState(string circuitName, int state)
    {
        if (string.IsNullOrWhiteSpace(circuitName))
        {
            throw new ArgumentException("Circuit name cannot be null or empty.", nameof(circuitName));
        }

        if (state < 0 || state > 2)
        {
            throw new ArgumentException("State must be 0 (Closed), 1 (Open), or 2 (Half-Open).", nameof(state));
        }

        _states[circuitName] = state;
    }

    /// <summary>
    /// Gets the current state of a circuit breaker.
    /// </summary>
    /// <param name="circuitName">The name of the circuit breaker.</param>
    /// <returns>The state value (0=Closed, 1=Open, 2=Half-Open). Defaults to 0 if circuit not found.</returns>
    public int GetState(string circuitName)
    {
        if (string.IsNullOrWhiteSpace(circuitName))
        {
            return ClosedState;
        }

        return _states.TryGetValue(circuitName, out var state) ? state : ClosedState;
    }

    /// <summary>
    /// Gets all tracked circuit breaker states.
    /// </summary>
    /// <returns>Dictionary of circuit name to state.</returns>
    public IReadOnlyDictionary<string, int> GetAllStates()
    {
        return _states.AsReadOnly();
    }

    /// <summary>
    /// Clears all tracked circuit breaker states.
    /// </summary>
    public void Clear()
    {
        _states.Clear();
    }
}
