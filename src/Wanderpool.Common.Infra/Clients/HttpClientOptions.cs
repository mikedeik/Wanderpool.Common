namespace Wanderpool.Common.Infra.Clients;

/// <summary>
/// Configuration options for Wanderpool HTTP clients.
/// </summary>
public class HttpClientOptions
{
    /// <summary>
    /// Base address for the HTTP client.
    /// </summary>
    public string BaseAddress { get; set; } = string.Empty;

    /// <summary>
    /// Name of the resilience pipeline to apply.
    /// </summary>
    public string ResiliencePipelineName { get; set; } = "default";

    /// <summary>
    /// HTTP request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Type of token provider for authentication.
    /// </summary>
    public Type? TokenProviderType { get; set; }
}
