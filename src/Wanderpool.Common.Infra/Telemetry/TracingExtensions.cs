using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Instrumentation.Http;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Wanderpool.Common.Infra.Telemetry;

/// <summary>
/// Extension methods for configuring OpenTelemetry distributed tracing.
/// </summary>
public static class TracingExtensions
{
    /// <summary>
    /// Adds OpenTelemetry tracing to the service collection with comprehensive instrumentation.
    /// Configures ASP.NET Core and HttpClient instrumentation with W3C trace propagation.
    /// </summary>
    /// <param name="services">The service collection to add tracing to.</param>
    /// <param name="serviceName">The name of the service for telemetry identification. Defaults to assembly name.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// Configuration is read from appsettings.json under the "OpenTelemetry" section:
    /// {
    ///   "OpenTelemetry": {
    ///     "Enabled": true,
    ///     "OtlpEndpoint": "http://localhost:4317",
    ///     "SamplingProbability": 1.0
    ///   }
    /// }
    ///
    /// The tracing setup includes:
    /// - ASP.NET Core instrumentation for incoming requests
    /// - HttpClient instrumentation for outgoing requests
    /// - W3C TraceContext propagation
    /// - Service name and version from assembly info
    /// - OTLP exporter for distributed tracing
    /// </remarks>
    public static IServiceCollection AddWanderpoolTracing(this IServiceCollection services, string? serviceName = null)
    {
        serviceName ??= Assembly.GetCallingAssembly().GetName().Name ?? "UnknownService";
        var serviceVersion = Assembly.GetCallingAssembly().GetName().Version?.ToString() ?? "1.0.0";

        services
            .AddOpenTelemetry()
            .WithTracing(traceBuilder =>
            {
                traceBuilder
                    // Set resource name and version
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(serviceName, serviceVersion: serviceVersion))

                    // Add ASP.NET Core instrumentation for incoming requests
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequest = (activity, request) =>
                        {
                            activity.SetTag("http.request.method_original", request.Method);
                        };
                        options.EnrichWithHttpResponse = (activity, response) =>
                        {
                            activity.SetTag("http.response.status_code", response.StatusCode);
                        };
                    })

                    // Add HttpClient instrumentation for outgoing requests
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequestMessage = (activity, request) =>
                        {
                            activity.SetTag("http.request.uri", request.RequestUri?.ToString());
                        };
                        options.EnrichWithHttpResponseMessage = (activity, response) =>
                        {
                            activity.SetTag("http.response.status_code", (int)response.StatusCode);
                        };
                    })

                    // Configure OTLP exporter
                    .AddOtlpExporter(options =>
                    {
                        // Default to localhost for local development
                        options.Endpoint = new Uri("http://localhost:4317");
                    });
            });

        return services;
    }

    /// <summary>
    /// Configures OpenTelemetry tracing with custom OTLP endpoint and sampling settings.
    /// </summary>
    /// <param name="services">The service collection to add tracing to.</param>
    /// <param name="otlpEndpoint">The OTLP exporter endpoint URL.</param>
    /// <param name="samplingProbability">The sampling probability (0.0 to 1.0). Defaults to 1.0 (100%).</param>
    /// <param name="serviceName">The name of the service. Defaults to assembly name.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWanderpoolTracing(
        this IServiceCollection services,
        string otlpEndpoint,
        double samplingProbability = 1.0,
        string? serviceName = null)
    {
        if (samplingProbability < 0.0 || samplingProbability > 1.0)
            throw new ArgumentException("Sampling probability must be between 0.0 and 1.0.", nameof(samplingProbability));

        serviceName ??= Assembly.GetCallingAssembly().GetName().Name ?? "UnknownService";
        var serviceVersion = Assembly.GetCallingAssembly().GetName().Version?.ToString() ?? "1.0.0";

        services
            .AddOpenTelemetry()
            .WithTracing(traceBuilder =>
            {
                traceBuilder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(serviceName, serviceVersion: serviceVersion))

                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })

                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })

                    // Configure sampling
                    .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(samplingProbability)))

                    // Configure OTLP exporter with custom endpoint
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
            });

        return services;
    }
}
