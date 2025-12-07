namespace Wanderpool.Common.Infra;

/// <summary>
/// Validates Wanderpool infrastructure configuration options.
/// </summary>
public static class WanderpoolConfigurationValidator
{
    /// <summary>
    /// Validates the Wanderpool options and throws if invalid.
    /// </summary>
    /// <param name="options">The options to validate.</param>
    /// <exception cref="ArgumentException">Thrown when options are invalid.</exception>
    public static void Validate(WanderpoolOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        ValidateServiceName(options.ServiceName);
        ValidateServiceVersion(options.ServiceVersion);
    }

    /// <summary>
    /// Validates the ServiceName property.
    /// </summary>
    /// <param name="serviceName">The service name to validate.</param>
    /// <exception cref="ArgumentException">Thrown when ServiceName is invalid.</exception>
    private static void ValidateServiceName(string? serviceName)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            throw new ArgumentException(
                "ServiceName cannot be null, empty, or whitespace. " +
                "ServiceName is required for telemetry (tracing, metrics) context. " +
                "Example: 'MyService', 'PaymentService', 'OrderProcessor'.",
                nameof(serviceName));
        }

        if (serviceName.Length > 256)
        {
            throw new ArgumentException(
                "ServiceName cannot exceed 256 characters.",
                nameof(serviceName));
        }
    }

    /// <summary>
    /// Validates the ServiceVersion property.
    /// </summary>
    /// <param name="serviceVersion">The service version to validate.</param>
    /// <exception cref="ArgumentException">Thrown when ServiceVersion is invalid.</exception>
    private static void ValidateServiceVersion(string? serviceVersion)
    {
        if (string.IsNullOrWhiteSpace(serviceVersion))
        {
            throw new ArgumentException(
                "ServiceVersion cannot be null, empty, or whitespace. " +
                "ServiceVersion is required for telemetry versioning. " +
                "Use semantic versioning format: '1.0.0', '2.1.5-beta', etc.",
                nameof(serviceVersion));
        }

        if (serviceVersion.Length > 256)
        {
            throw new ArgumentException(
                "ServiceVersion cannot exceed 256 characters.",
                nameof(serviceVersion));
        }
    }
}
