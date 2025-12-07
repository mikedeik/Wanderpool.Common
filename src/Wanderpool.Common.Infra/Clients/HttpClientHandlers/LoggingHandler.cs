using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Wanderpool.Common.Infra.Clients.HttpClientHandlers;

/// <summary>
/// DelegatingHandler that logs outbound HTTP requests and responses.
/// Logs request details before sending and response details after receiving.
/// </summary>
public class LoggingHandler : DelegatingHandler
{
    private readonly ILogger<LoggingHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the LoggingHandler class.
    /// </summary>
    /// <param name="logger">Logger for recording HTTP request/response details.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public LoggingHandler(ILogger<LoggingHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sends an HTTP request and logs request/response details.
    /// </summary>
    /// <param name="request">The HTTP request to send.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The HTTP response message from the server.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Log the outbound request
            LogRequest(request);

            // Send the request
            var response = await base.SendAsync(request, cancellationToken);

            stopwatch.Stop();

            // Log the response
            LogResponse(request, response, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Log the exception
            LogException(request, ex, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    /// <summary>
    /// Logs the outbound HTTP request details.
    /// </summary>
    private void LogRequest(HttpRequestMessage request)
    {
        _logger.LogInformation(
            "Outbound HTTP {HttpMethod} request to {RequestUri}",
            request.Method,
            request.RequestUri);
    }

    /// <summary>
    /// Logs the HTTP response details with appropriate log level based on status code.
    /// </summary>
    private void LogResponse(HttpRequestMessage request, HttpResponseMessage response, long elapsedMilliseconds)
    {
        var logLevel = DetermineLogLevel(response.StatusCode);

        _logger.Log(
            logLevel,
            "Outbound HTTP {StatusCode} response from {RequestUri} | Duration: {ElapsedMilliseconds}ms",
            (int)response.StatusCode,
            request.RequestUri,
            elapsedMilliseconds);
    }

    /// <summary>
    /// Logs exception details that occurred during the HTTP request.
    /// </summary>
    private void LogException(HttpRequestMessage request, Exception exception, long elapsedMilliseconds)
    {
        _logger.LogError(
            exception,
            "Outbound HTTP request to {RequestUri} failed with exception | Duration: {ElapsedMilliseconds}ms | Exception: {ExceptionType}",
            request.RequestUri,
            elapsedMilliseconds,
            exception.GetType().Name);
    }

    /// <summary>
    /// Determines the appropriate log level based on HTTP status code.
    /// </summary>
    private static LogLevel DetermineLogLevel(System.Net.HttpStatusCode statusCode)
    {
        return (int)statusCode switch
        {
            >= 200 and < 300 => LogLevel.Information,
            >= 300 and < 400 => LogLevel.Information,
            >= 400 and < 500 => LogLevel.Warning,
            >= 500 => LogLevel.Error,
            _ => LogLevel.Information
        };
    }
}
