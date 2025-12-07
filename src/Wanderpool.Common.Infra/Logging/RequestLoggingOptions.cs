namespace Wanderpool.Common.Infra.Logging;

/// <summary>
/// Configuration options for HTTP request/response logging behavior.
/// </summary>
public class RequestLoggingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to log request bodies.
    /// Default is false for security reasons (can contain sensitive data).
    /// </summary>
    public bool EnableRequestBodyLogging { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to log response bodies.
    /// Default is false for security reasons (can contain sensitive data).
    /// </summary>
    public bool EnableResponseBodyLogging { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum size in bytes of request/response bodies to log.
    /// Bodies larger than this limit will be truncated in logs.
    /// Default is 4096 bytes (4 KB).
    /// </summary>
    public int MaxBodySizeLogged { get; set; } = 4096;

    /// <summary>
    /// Gets or sets a value indicating whether to include query string parameters in logs.
    /// Default is true.
    /// </summary>
    public bool IncludeQueryString { get; set; } = true;

    /// <summary>
    /// Gets a collection of HTTP header names that should be redacted in logs.
    /// These headers are considered sensitive and their values will be masked.
    /// </summary>
    public ICollection<string> SensitiveHeaders { get; } = new List<string>
    {
        "Authorization",
        "X-Api-Key",
        "X-Access-Token",
        "Cookie",
        "Set-Cookie",
        "X-CSRF-Token",
        "X-Auth-Token",
        "Authorization-Token"
    };
}
