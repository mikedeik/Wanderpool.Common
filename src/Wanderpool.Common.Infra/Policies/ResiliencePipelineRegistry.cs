using System.Collections.Concurrent;

namespace Wanderpool.Common.Infra.Policies;

/// <summary>
/// Thread-safe registry for storing and retrieving named resilience pipelines.
/// Allows applications to define multiple resilience pipelines with different configurations.
/// </summary>
public class ResiliencePipelineRegistry
{
    private readonly ConcurrentDictionary<string, ResilienceOptions> _pipelines = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers a named resilience pipeline with the given configuration.
    /// If a pipeline with the same name already exists, it will be overwritten.
    /// </summary>
    /// <param name="name">The unique name for the pipeline.</param>
    /// <param name="options">The resilience options for this pipeline.</param>
    /// <exception cref="ArgumentNullException">Thrown when name or options is null.</exception>
    public void Register(string name, ResilienceOptions options)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(options);

        _pipelines[name] = options;
        
    }

    /// <summary>
    /// Retrieves a registered resilience pipeline by name.
    /// </summary>
    /// <param name="name">The name of the pipeline to retrieve.</param>
    /// <returns>The resilience options for the named pipeline.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the pipeline name is not registered.</exception>
    public ResilienceOptions Get(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        
            if (_pipelines.TryGetValue(name, out var options))
            {
                return options;
            }

        throw new KeyNotFoundException($"Resilience pipeline '{name}' is not registered. Available pipelines: {string.Join(", ", GetRegisteredNames())}");
    }

    /// <summary>
    /// Attempts to retrieve a registered resilience pipeline by name.
    /// </summary>
    /// <param name="name">The name of the pipeline to retrieve.</param>
    /// <param name="options">When this method returns, contains the resilience options for the named pipeline, or null if not found.</param>
    /// <returns>true if the pipeline is registered; otherwise, false.</returns>
    public bool TryGet(string name, out ResilienceOptions? options)
    {
        ArgumentNullException.ThrowIfNull(name);
        return _pipelines.TryGetValue(name, out options);
        
    }

    /// <summary>
    /// Gets all registered pipeline names.
    /// </summary>
    /// <returns>A collection of registered pipeline names.</returns>
    public IReadOnlyCollection<string> GetRegisteredNames()
    {

        return _pipelines.Keys.ToList().AsReadOnly();
        
    }

    /// <summary>
    /// Checks if a pipeline with the given name is registered.
    /// </summary>
    /// <param name="name">The name of the pipeline to check.</param>
    /// <returns>true if the pipeline is registered; otherwise, false.</returns>
    public bool Contains(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        return _pipelines.ContainsKey(name);
        
    }

    /// <summary>
    /// Removes a registered pipeline by name.
    /// </summary>
    /// <param name="name">The name of the pipeline to remove.</param>
    /// <returns>true if the pipeline was removed; otherwise, false.</returns>
    public bool Unregister(string name)
    {
        ArgumentNullException.ThrowIfNull(name);


        return _pipelines.Remove(name, out var _);
        
    }

    /// <summary>
    /// Clears all registered pipelines.
    /// </summary>
    public void Clear()
    {

        _pipelines.Clear();
        
    }

    /// <summary>
    /// Gets the total number of registered pipelines.
    /// </summary>
    public int Count
    {
        get
        {
            return _pipelines.Count;
        }
    }
}
