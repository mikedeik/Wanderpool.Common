using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Exporter;
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
    ///     "Endpoint": "http://localhost:4317",
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
                            activity.EnrichWithCorrelationId(request?.HttpContext);
                            activity.EnrichWithRequestBodySize(request);
                            activity.EnrichWithClientIp(request);
                            activity.EnrichWithUserAgent(request);
                            activity.EnrichWithRequestPath(request);
                            activity.EnrichWithContentType(request);
                        };
                        options.EnrichWithHttpResponse = (activity, response) =>
                        {
                            activity.SetTag("http.response.status_code", response.StatusCode);
                            activity.EnrichWithResponseBodySize(response);
                            activity.EnrichWithResponseContentType(response);
                        };
                    })

                    // Add HttpClient instrumentation for outgoing requests
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequestMessage = (activity, request) =>
                        {
                            activity.SetTag("http.request.uri", request.RequestUri?.ToString());
                            if (request.Headers != null)
                            {
                                var contentLength = request.Content?.Headers?.ContentLength;
                                if (contentLength.HasValue && contentLength.Value > 0)
                                {
                                    activity.SetTag("http.request.body.size", contentLength.Value);
                                }

                                if (request.Headers.TryGetValues("User-Agent", out var userAgentValues))
                                {
                                    var userAgent = userAgentValues.FirstOrDefault();
                                    if (!string.IsNullOrEmpty(userAgent))
                                    {
                                        activity.SetTag("http.request.user_agent", userAgent);
                                    }
                                }
                            }
                        };
                        options.EnrichWithHttpResponseMessage = (activity, response) =>
                        {
                            activity.SetTag("http.response.status_code", (int)response.StatusCode);
                            var contentLength = response.Content?.Headers?.ContentLength;
                            if (contentLength.HasValue && contentLength.Value > 0)
                            {
                                activity.SetTag("http.response.body.size", contentLength.Value);
                            }
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

    /// <summary>
    /// Configures OpenTelemetry tracing with Options pattern for configuration.
    /// Reads configuration from appsettings.json and uses IOptions for dependency injection.
    /// </summary>
    /// <param name="services">The service collection to add tracing to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="serviceName">The name of the service. Defaults to assembly name.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// This method configures OpenTelemetryConfiguration from the "OpenTelemetry" section
    /// in appsettings.json and uses the Options pattern for dependency injection.
    ///
    /// Configuration example:
    /// {
    ///   "OpenTelemetry": {
    ///     "Enabled": true,
    ///     "Endpoint": "http://localhost:4317",
    ///     "SamplingProbability": 1.0
    ///   }
    /// }
    /// </remarks>
    public static IServiceCollection AddWanderpoolTracingWithConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        string? serviceName = null)
    {
        // Configure OpenTelemetryConfiguration from appsettings
        services.Configure<OpenTelemetryConfiguration>(configuration.GetSection(OpenTelemetryConfiguration.Name));

        // Register the tracing with options
        services.AddSingleton<IConfigureOptions<TracerProviderBuilder>>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<OpenTelemetryConfiguration>>().Value;
            return new ConfigureTracerProviderBuilder(options, serviceName);
        });

        // Add OpenTelemetry with custom configuration
        services
            .AddOpenTelemetry()
            .WithTracing(traceBuilder =>
            {
                ConfigureTracingWithOptions(traceBuilder, provider: null, serviceName: serviceName);
            });

        return services;
    }

    /// <summary>
    /// Configures OpenTelemetry tracing with environment-aware exporters using Options pattern.
    /// </summary>
    /// <param name="services">The service collection to add tracing to.</param>
    /// <param name="environment">The host environment for determining which exporters to use.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="serviceName">The name of the service. Defaults to assembly name.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// Configuration varies by environment:
    /// - Development: Includes Console exporter for debugging + OTLP exporter
    /// - Production: OTLP exporter only, no debug exporters
    /// </remarks>
    public static IServiceCollection AddWanderpoolTracingWithExporters(
        this IServiceCollection services,
        IWebHostEnvironment environment,
        IConfiguration configuration,
        string? serviceName = null)
    {
        // Configure OpenTelemetryConfiguration from appsettings
        services.Configure<OpenTelemetryConfiguration>(configuration.GetSection(OpenTelemetryConfiguration.Name));

        serviceName ??= Assembly.GetCallingAssembly().GetName().Name ?? "UnknownService";
        var serviceVersion = Assembly.GetCallingAssembly().GetName().Version?.ToString() ?? "1.0.0";

        services
            .AddOpenTelemetry()
            .WithTracing(traceBuilder =>
            {
                var otelOptions = new OpenTelemetryConfiguration();
                configuration.GetSection(OpenTelemetryConfiguration.Name).Bind(otelOptions);

                traceBuilder
                    // Set resource name and version
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(serviceName, serviceVersion: serviceVersion))

                    // Add ASP.NET Core instrumentation
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequest = (activity, request) =>
                        {
                            activity.SetTag("http.request.method_original", request.Method);
                            activity.EnrichWithCorrelationId(request?.HttpContext);
                            activity.EnrichWithRequestBodySize(request);
                            activity.EnrichWithClientIp(request);
                            activity.EnrichWithUserAgent(request);
                            activity.EnrichWithRequestPath(request);
                            activity.EnrichWithContentType(request);
                            activity.EnrichWithEnvironmentInfo(environment?.EnvironmentName);
                        };
                        options.EnrichWithHttpResponse = (activity, response) =>
                        {
                            activity.SetTag("http.response.status_code", response.StatusCode);
                            activity.EnrichWithResponseBodySize(response);
                            activity.EnrichWithResponseContentType(response);
                        };
                    })

                    // Add HttpClient instrumentation
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequestMessage = (activity, request) =>
                        {
                            activity.SetTag("http.request.uri", request.RequestUri?.ToString());
                            if (request.Headers != null)
                            {
                                var contentLength = request.Content?.Headers?.ContentLength;
                                if (contentLength.HasValue && contentLength.Value > 0)
                                {
                                    activity.SetTag("http.request.body.size", contentLength.Value);
                                }

                                if (request.Headers.TryGetValues("User-Agent", out var userAgentValues))
                                {
                                    var userAgent = userAgentValues.FirstOrDefault();
                                    if (!string.IsNullOrEmpty(userAgent))
                                    {
                                        activity.SetTag("http.request.user_agent", userAgent);
                                    }
                                }
                            }
                        };
                        options.EnrichWithHttpResponseMessage = (activity, response) =>
                        {
                            activity.SetTag("http.response.status_code", (int)response.StatusCode);
                            var contentLength = response.Content?.Headers?.ContentLength;
                            if (contentLength.HasValue && contentLength.Value > 0)
                            {
                                activity.SetTag("http.response.body.size", contentLength.Value);
                            }
                        };
                    });

                // Configure exporters based on environment
                if (environment.IsDevelopment())
                {
                    // Development: Add console exporter for debugging
                    traceBuilder.AddConsoleExporter();
                }

                // Always add OTLP exporter (production and development)
                traceBuilder.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otelOptions.Endpoint);
                });

                // Configure sampling if specified
                if (otelOptions.SamplingProbability < 1.0)
                {
                    traceBuilder.SetSampler(new ParentBasedSampler(
                        new TraceIdRatioBasedSampler(otelOptions.SamplingProbability)));
                }
            });

        return services;
    }

    /// <summary>
    /// Helper method to configure tracing with options.
    /// </summary>
    private static void ConfigureTracingWithOptions(
        TracerProviderBuilder traceBuilder,
        IServiceProvider? provider = null,
        string? serviceName = null)
    {
        serviceName ??= Assembly.GetCallingAssembly().GetName().Name ?? "UnknownService";
        var serviceVersion = Assembly.GetCallingAssembly().GetName().Version?.ToString() ?? "1.0.0";

        traceBuilder
            // Set resource name and version
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService(serviceName, serviceVersion: serviceVersion))

            // Add ASP.NET Core instrumentation
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
            })

            // Add HttpClient instrumentation
            .AddHttpClientInstrumentation(options =>
            {
                options.RecordException = true;
            });
    }

    /// <summary>
    /// Configures OpenTelemetry tracing with configurable exporters from appsettings.json.
    /// Supports multiple exporters: OTLP, Jaeger, Zipkin, and Console.
    /// </summary>
    /// <param name="services">The service collection to add tracing to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="serviceName">The name of the service. Defaults to assembly name.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// Configuration example for multiple exporters:
    /// {
    ///   "OpenTelemetry": {
    ///     "Enabled": true,
    ///     "SamplingProbability": 1.0,
    ///     "Exporters": {
    ///       "OTLP": {
    ///         "Type": "otlp",
    ///         "Endpoint": "http://localhost:4317",
    ///         "Enabled": true
    ///       },
    ///       "Jaeger": {
    ///         "Type": "jaeger",
    ///         "Endpoint": "http://localhost:14268/api/traces",
    ///         "Enabled": true
    ///       },
    ///       "Console": {
    ///         "Type": "console",
    ///         "Enabled": false
    ///       }
    ///     }
    ///   }
    /// }
    /// </remarks>
    public static IServiceCollection AddWanderpoolTracingWithConfigurableExporters(
        this IServiceCollection services,
        IConfiguration configuration,
        string? serviceName = null)
    {
        // Configure OpenTelemetryConfiguration from appsettings
        services.Configure<OpenTelemetryConfiguration>(configuration.GetSection(OpenTelemetryConfiguration.Name));

        serviceName ??= Assembly.GetCallingAssembly().GetName().Name ?? "UnknownService";
        var serviceVersion = Assembly.GetCallingAssembly().GetName().Version?.ToString() ?? "1.0.0";

        services
            .AddOpenTelemetry()
            .WithTracing(traceBuilder =>
            {
                var otelOptions = new OpenTelemetryConfiguration();
                configuration.GetSection(OpenTelemetryConfiguration.Name).Bind(otelOptions);

                traceBuilder
                    // Set resource name and version
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(serviceName, serviceVersion: serviceVersion))

                    // Add ASP.NET Core instrumentation
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequest = (activity, request) =>
                        {
                            activity.SetTag("http.request.method_original", request.Method);
                            activity.EnrichWithCorrelationId(request?.HttpContext);
                            activity.EnrichWithRequestBodySize(request);
                            activity.EnrichWithClientIp(request);
                            activity.EnrichWithUserAgent(request);
                            activity.EnrichWithRequestPath(request);
                            activity.EnrichWithContentType(request);
                        };
                        options.EnrichWithHttpResponse = (activity, response) =>
                        {
                            activity.SetTag("http.response.status_code", response.StatusCode);
                            activity.EnrichWithResponseBodySize(response);
                            activity.EnrichWithResponseContentType(response);
                        };
                    })

                    // Add HttpClient instrumentation
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequestMessage = (activity, request) =>
                        {
                            activity.SetTag("http.request.uri", request.RequestUri?.ToString());
                            if (request.Headers != null)
                            {
                                var contentLength = request.Content?.Headers?.ContentLength;
                                if (contentLength.HasValue && contentLength.Value > 0)
                                {
                                    activity.SetTag("http.request.body.size", contentLength.Value);
                                }

                                if (request.Headers.TryGetValues("User-Agent", out var userAgentValues))
                                {
                                    var userAgent = userAgentValues.FirstOrDefault();
                                    if (!string.IsNullOrEmpty(userAgent))
                                    {
                                        activity.SetTag("http.request.user_agent", userAgent);
                                    }
                                }
                            }
                        };
                        options.EnrichWithHttpResponseMessage = (activity, response) =>
                        {
                            activity.SetTag("http.response.status_code", (int)response.StatusCode);
                            var contentLength = response.Content?.Headers?.ContentLength;
                            if (contentLength.HasValue && contentLength.Value > 0)
                            {
                                activity.SetTag("http.response.body.size", contentLength.Value);
                            }
                        };
                    })

                    // Configure exporters from configuration
                    .AddConfiguredExporters(otelOptions);

                // Configure sampling if specified
                if (otelOptions.SamplingProbability < 1.0)
                {
                    traceBuilder.SetSampler(new ParentBasedSampler(
                        new TraceIdRatioBasedSampler(otelOptions.SamplingProbability)));
                }
            });

        return services;
    }

    /// <summary>
    /// Configuration helper for TracerProviderBuilder.
    /// </summary>
    private class ConfigureTracerProviderBuilder : IConfigureOptions<TracerProviderBuilder>
    {
        private readonly OpenTelemetryConfiguration _options;
        private readonly string? _serviceName;

        public ConfigureTracerProviderBuilder(OpenTelemetryConfiguration options, string? serviceName)
        {
            _options = options;
            _serviceName = serviceName;
        }

        public void Configure(TracerProviderBuilder builder)
        {
            ConfigureTracingWithOptions(builder, serviceName: _serviceName);
        }
    }
}
