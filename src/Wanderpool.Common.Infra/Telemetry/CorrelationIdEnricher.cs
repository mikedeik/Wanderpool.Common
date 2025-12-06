using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Wanderpool.Common.Infra.Telemetry;

/// <summary>
/// Serilog enricher that adds correlation ID to all log entries.
/// Enables correlation of logs across requests and services.
/// </summary>
public class CorrelationIdEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Creates a new instance of CorrelationIdEnricher.
    /// </summary>
    /// <param name="httpContextAccessor">HTTP context accessor for retrieving request context.</param>
    public CorrelationIdEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <summary>
    /// Enriches the log event with correlation ID.
    /// </summary>
    /// <param name="logEvent">The log event to enrich.</param>
    /// <param name="propertyFactory">Factory for creating log event properties.</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        // Try to get correlation ID from HttpContext items (set by middleware)
        if (httpContext.Items.TryGetValue("CorrelationId", out var correlationId) && correlationId is string id)
        {
            var property = propertyFactory.CreateProperty("CorrelationId", id);
            logEvent.AddPropertyIfAbsent(property);
        }
    }
}
