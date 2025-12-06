using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Http;

namespace Wanderpool.Common.Infra.Telemetry;

/// <summary>
/// Provides enrichment methods for OpenTelemetry spans with business and technical context.
/// </summary>
public static class TracingEnricher
{
    /// <summary>
    /// Enriches HTTP request spans with correlation ID from HttpContext.
    /// </summary>
    /// <param name="activity">The activity to enrich.</param>
    /// <param name="httpContext">The HTTP context containing the correlation ID.</param>
    public static void EnrichWithCorrelationId(this Activity? activity, HttpContext? httpContext)
    {
        if (activity == null || httpContext == null)
            return;

        if (httpContext.Items.TryGetValue("CorrelationId", out var correlationId) && correlationId is string id)
        {
            activity.SetTag("correlation_id", id);
        }
    }

    /// <summary>
    /// Enriches HTTP request spans with request body size information.
    /// </summary>
    /// <param name="activity">The activity to enrich.</param>
    /// <param name="request">The HTTP request to extract body size from.</param>
    public static void EnrichWithRequestBodySize(this Activity? activity, HttpRequest? request)
    {
        if (activity == null || request == null)
            return;

        if (request.ContentLength.HasValue && request.ContentLength.Value > 0)
        {
            activity.SetTag("http.request.body.size", request.ContentLength.Value);
        }
    }

    /// <summary>
    /// Enriches HTTP response spans with response body size information.
    /// </summary>
    /// <param name="activity">The activity to enrich.</param>
    /// <param name="response">The HTTP response to extract body size from.</param>
    public static void EnrichWithResponseBodySize(this Activity? activity, HttpResponse? response)
    {
        if (activity == null || response == null)
            return;

        if (response.ContentLength.HasValue && response.ContentLength.Value > 0)
        {
            activity.SetTag("http.response.body.size", response.ContentLength.Value);
        }
    }

    /// <summary>
    /// Enriches HTTP request spans with client IP address.
    /// </summary>
    /// <param name="activity">The activity to enrich.</param>
    /// <param name="request">The HTTP request to extract client IP from.</param>
    public static void EnrichWithClientIp(this Activity? activity, HttpRequest? request)
    {
        if (activity == null || request?.HttpContext?.Connection?.RemoteIpAddress == null)
            return;

        var clientIp = request.HttpContext.Connection.RemoteIpAddress.ToString();
        activity.SetTag("http.client_ip", clientIp);
    }

    /// <summary>
    /// Enriches HTTP request spans with user agent information.
    /// </summary>
    /// <param name="activity">The activity to enrich.</param>
    /// <param name="request">The HTTP request to extract user agent from.</param>
    public static void EnrichWithUserAgent(this Activity? activity, HttpRequest? request)
    {
        if (activity == null || request == null)
            return;

        if (request.Headers.TryGetValue("User-Agent", out var userAgent) && !userAgent.ToString().IsNullOrEmpty())
        {
            activity.SetTag("http.user_agent", userAgent.ToString());
        }
    }

    /// <summary>
    /// Enriches HTTP request spans with request path and query information.
    /// </summary>
    /// <param name="activity">The activity to enrich.</param>
    /// <param name="request">The HTTP request to extract path from.</param>
    public static void EnrichWithRequestPath(this Activity? activity, HttpRequest? request)
    {
        if (activity == null || request == null)
            return;

        var path = request.Path.ToString();
        if (!path.IsNullOrEmpty())
        {
            activity.SetTag("http.target", path);
        }

        if (!request.QueryString.ToString().IsNullOrEmpty())
        {
            activity.SetTag("http.query_string", request.QueryString.ToString());
        }
    }

    /// <summary>
    /// Enriches spans with environment information (machine name, environment name).
    /// </summary>
    /// <param name="activity">The activity to enrich.</param>
    /// <param name="environmentName">The name of the environment (Development, Production, etc.).</param>
    public static void EnrichWithEnvironmentInfo(this Activity? activity, string? environmentName = null)
    {
        if (activity == null)
            return;

        activity.SetTag("host.name", Environment.MachineName);

        if (!environmentName.IsNullOrEmpty())
        {
            activity.SetTag("deployment.environment", environmentName);
        }
    }

    /// <summary>
    /// Enriches exception spans with detailed exception information.
    /// </summary>
    /// <param name="activity">The activity to enrich.</param>
    /// <param name="exception">The exception to extract details from.</param>
    public static void EnrichWithExceptionDetails(this Activity? activity, Exception? exception)
    {
        if (activity == null || exception == null)
            return;

        activity.SetTag("exception.type", exception.GetType().FullName);
        activity.SetTag("exception.message", exception.Message);

        if (!exception.StackTrace.IsNullOrEmpty())
        {
            activity.SetTag("exception.stacktrace", exception.StackTrace);
        }

        // Add inner exception information if present
        if (exception.InnerException != null)
        {
            activity.SetTag("exception.inner_type", exception.InnerException.GetType().FullName);
            activity.SetTag("exception.inner_message", exception.InnerException.Message);
        }
    }

    /// <summary>
    /// Enriches HTTP request spans with content type information.
    /// </summary>
    /// <param name="activity">The activity to enrich.</param>
    /// <param name="request">The HTTP request to extract content type from.</param>
    public static void EnrichWithContentType(this Activity? activity, HttpRequest? request)
    {
        if (activity == null || request?.ContentType == null)
            return;

        activity.SetTag("http.content_type", request.ContentType);
    }

    /// <summary>
    /// Enriches HTTP response spans with response content type.
    /// </summary>
    /// <param name="activity">The activity to enrich.</param>
    /// <param name="response">The HTTP response to extract content type from.</param>
    public static void EnrichWithResponseContentType(this Activity? activity, HttpResponse? response)
    {
        if (activity == null || response?.ContentType == null)
            return;

        activity.SetTag("http.response.content_type", response.ContentType);
    }

    /// <summary>
    /// Helper to check if string is null or empty.
    /// </summary>
    private static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value);
}
