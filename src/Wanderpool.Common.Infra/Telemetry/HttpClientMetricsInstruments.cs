using System.Diagnostics.Metrics;

namespace Wanderpool.Common.Infra.Telemetry;

/// <summary>
/// Contains custom metric instruments for HTTP client request tracking.
/// </summary>
public sealed class HttpClientMetricsInstruments
{
    /// <summary>
    /// Counter for tracking total HTTP client requests.
    /// Tagged by: client_name, http_method, http_status_code
    /// Unit: requests
    /// </summary>
    public required Counter<long> RequestsCounter { get; init; }

    /// <summary>
    /// Histogram for tracking HTTP client request duration.
    /// Tagged by: client_name, http_method, http_status_code
    /// Unit: milliseconds
    /// </summary>
    public required Histogram<double> RequestDurationHistogram { get; init; }
}
