using System.Diagnostics;
using Wanderpool.Common.Infra.Telemetry;

namespace Wanderpool.Common.Infra.Clients.HttpClientHandlers;

/// <summary>
/// DelegatingHandler that records HTTP client request metrics.
/// Records request count and duration using OpenTelemetry metric instruments.
/// </summary>
public class HttpClientMetricsHandler : DelegatingHandler
{
    private readonly HttpClientMetricsInstruments _instruments;
    private readonly string _clientName;

    /// <summary>
    /// Initializes a new instance of the HttpClientMetricsHandler.
    /// </summary>
    /// <param name="instruments">The metric instruments for recording metrics.</param>
    /// <param name="clientName">The name of the HTTP client. Defaults to "Unknown" if not provided.</param>
    public HttpClientMetricsHandler(HttpClientMetricsInstruments instruments, string? clientName = null)
    {
        _instruments = instruments ?? throw new ArgumentNullException(nameof(instruments));
        _clientName = clientName ?? "Unknown";
    }

    /// <summary>
    /// Sends an HTTP request and records metrics about the request.
    /// </summary>
    /// <param name="request">The HTTP request message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The HTTP response message.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var stopwatch = Stopwatch.StartNew();
        var method = request.Method?.Method ?? "UNKNOWN";
        var statusCode = "unknown";

        try
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            statusCode = ((int)response.StatusCode).ToString();

            RecordMetrics(method, statusCode, stopwatch.Elapsed.TotalSeconds);
            return response;
        }
        catch (HttpRequestException)
        {
            statusCode = "error";
            RecordMetrics(method, statusCode, stopwatch.Elapsed.TotalSeconds);
            throw;
        }
        catch (OperationCanceledException)
        {
            statusCode = "cancelled";
            RecordMetrics(method, statusCode, stopwatch.Elapsed.TotalSeconds);
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    /// <summary>
    /// Records metrics for the HTTP request.
    /// </summary>
    private void RecordMetrics(string method, string statusCode, double durationSeconds)
    {
        var tags = new[]
        {
            new KeyValuePair<string, object?>("client_name", _clientName),
            new KeyValuePair<string, object?>("http_method", method),
            new KeyValuePair<string, object?>("http_status_code", statusCode)
        };

        // Record request count
        _instruments.RequestsCounter.Add(1, tags);

        // Record request duration
        _instruments.RequestDurationHistogram.Record(durationSeconds, tags);
    }
}
