using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Wanderpool.Common.Infra.Logging;

/// <summary>
/// Middleware for logging HTTP requests and responses with configurable options.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly IOptions<RequestLoggingOptions> _options;

    /// <summary>
    /// Initializes a new instance of the RequestLoggingMiddleware class.
    /// </summary>
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger, IOptions<RequestLoggingOptions> options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Invokes the middleware to log incoming request and response.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            // Log the incoming request
            LogRequest(context);

            // Replace response body stream to capture response
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            try
            {
                // Call the next middleware
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                // Log the response
                await LogResponse(context, memoryStream, stopwatch.ElapsedMilliseconds);

                // Copy the response back to the original stream
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBodyStream);
            }
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    /// <summary>
    /// Logs the incoming HTTP request details.
    /// </summary>
    private void LogRequest(HttpContext context)
    {
        var request = context.Request;
        var options = _options.Value;
        var correlationId = context.Items.ContainsKey("CorrelationId") ? context.Items["CorrelationId"]?.ToString() : "N/A";

        var pathAndQuery = request.Path.HasValue ? request.Path.Value : "/";
        if (options.IncludeQueryString && request.QueryString.HasValue)
        {
            pathAndQuery += request.QueryString.Value;
        }

        // Log headers with redaction for sensitive ones
        var headersInfo = GetRedactedHeadersInfo(request.Headers, options);

        _logger.LogInformation(
            "HTTP {HttpMethod} request to {Path} | Headers: {Headers} | CorrelationId: {CorrelationId}",
            request.Method,
            pathAndQuery,
            headersInfo,
            correlationId);
    }

    /// <summary>
    /// Gets header information with sensitive headers redacted.
    /// </summary>
    private string GetRedactedHeadersInfo(IHeaderDictionary headers, RequestLoggingOptions options)
    {
        if (!headers.Any())
        {
            return "none";
        }

        var redactedHeaders = new List<string>();
        var sensitiveHeadersLower = options.SensitiveHeaders
            .Select(h => h.ToLowerInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var header in headers)
        {
            var headerName = header.Key;
            var isSensitive = sensitiveHeadersLower.Contains(headerName.ToLowerInvariant());
            var value = isSensitive ? "[REDACTED]" : header.Value.ToString();
            redactedHeaders.Add($"{headerName}={value}");
        }

        return string.Join(", ", redactedHeaders);
    }

    /// <summary>
    /// Logs the HTTP response details.
    /// </summary>
    private async Task LogResponse(HttpContext context, MemoryStream responseBody, long elapsedMilliseconds)
    {
        var response = context.Response;
        var options = _options.Value;
        var correlationId = context.Items.ContainsKey("CorrelationId") ? context.Items["CorrelationId"]?.ToString() : "N/A";

        var logLevel = DetermineLogLevel(response.StatusCode);
        var statusCodeCategory = response.StatusCode / 100;

        // Get response body if logging is enabled
        string responseContent = string.Empty;
        if (options.EnableResponseBodyLogging && responseBody.Length > 0)
        {
            responseBody.Position = 0;
            using var reader = new StreamReader(responseBody);
            responseContent = await reader.ReadToEndAsync();

            // Truncate if needed
            if (responseContent.Length > options.MaxBodySizeLogged)
            {
                responseContent = responseContent.Substring(0, options.MaxBodySizeLogged) + "... [TRUNCATED]";
            }
        }

        using (_logger.BeginScope(new Dictionary<string, object?> { { "CorrelationId", correlationId ?? string.Empty } }))
        {
            if (options.EnableResponseBodyLogging && !string.IsNullOrEmpty(responseContent))
            {
                _logger.Log(logLevel,
                    "HTTP {StatusCode} response | Duration: {ElapsedMilliseconds}ms | Body: {ResponseBody} | CorrelationId: {CorrelationId}",
                    response.StatusCode,
                    elapsedMilliseconds,
                    responseContent,
                    correlationId);
            }
            else
            {
                _logger.Log(logLevel,
                    "HTTP {StatusCode} response | Duration: {ElapsedMilliseconds}ms | CorrelationId: {CorrelationId}",
                    response.StatusCode,
                    elapsedMilliseconds,
                    correlationId);
            }
        }
    }

    /// <summary>
    /// Determines the appropriate log level based on HTTP status code.
    /// </summary>
    private static LogLevel DetermineLogLevel(int statusCode)
    {
        return statusCode switch
        {
            >= 200 and < 300 => LogLevel.Information,
            >= 300 and < 400 => LogLevel.Information,
            >= 400 and < 500 => LogLevel.Warning,
            >= 500 => LogLevel.Error,
            _ => LogLevel.Information
        };
    }
}
