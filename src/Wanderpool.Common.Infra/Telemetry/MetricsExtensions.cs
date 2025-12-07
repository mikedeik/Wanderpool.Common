using System.Diagnostics.Metrics;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;

namespace Wanderpool.Common.Infra.Telemetry;

/// <summary>
/// Extension methods for configuring OpenTelemetry metrics.
/// </summary>
public static class MetricsExtensions
{
    /// <summary>
    /// Adds OpenTelemetry metrics collection to the service collection with default configuration.
    /// Configures ASP.NET Core, HttpClient, and runtime metrics with OTLP exporter.
    /// </summary>
    /// <param name="services">The service collection to add metrics to.</param>
    /// <param name="serviceName">The name of the service. Defaults to assembly name.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// The metrics setup includes:
    /// - ASP.NET Core metrics (request duration, count, etc.)
    /// - HttpClient metrics (outbound request stats)
    /// - Runtime metrics (GC, thread pool, etc.)
    /// - OTLP exporter for metrics
    /// - Metrics exported every 60 seconds
    /// </remarks>
    public static IServiceCollection AddWanderpoolMetrics(
        this IServiceCollection services,
        string? serviceName = null)
    {
        serviceName ??= Assembly.GetCallingAssembly().GetName().Name ?? "UnknownService";
        var serviceVersion = Assembly.GetCallingAssembly().GetName().Version?.ToString() ?? "1.0.0";

        services
            .AddOpenTelemetry()
            .WithMetrics(metricsBuilder =>
            {
                metricsBuilder
                    .AddMeter("OpenTelemetry.Instrumentation.AspNetCore")
                    .AddMeter("OpenTelemetry.Instrumentation.Http")
                    .AddMeter("OpenTelemetry.Instrumentation.Runtime")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri("http://localhost:4317");
                    });
            });

        return services;
    }

    /// <summary>
    /// Configures OpenTelemetry metrics with custom OTLP endpoint.
    /// </summary>
    /// <param name="services">The service collection to add metrics to.</param>
    /// <param name="otlpEndpoint">The OTLP exporter endpoint URL.</param>
    /// <param name="serviceName">The name of the service. Defaults to assembly name.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWanderpoolMetrics(
        this IServiceCollection services,
        string otlpEndpoint,
        string? serviceName = null)
    {
        serviceName ??= Assembly.GetCallingAssembly().GetName().Name ?? "UnknownService";
        var serviceVersion = Assembly.GetCallingAssembly().GetName().Version?.ToString() ?? "1.0.0";

        services
            .AddOpenTelemetry()
            .WithMetrics(metricsBuilder =>
            {
                metricsBuilder
                    .AddMeter("OpenTelemetry.Instrumentation.AspNetCore")
                    .AddMeter("OpenTelemetry.Instrumentation.Http")
                    .AddMeter("OpenTelemetry.Instrumentation.Runtime")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
            });

        return services;
    }

    /// <summary>
    /// Configures OpenTelemetry metrics with configuration-driven setup.
    /// Supports both OTLP and Prometheus exporters.
    /// </summary>
    /// <param name="services">The service collection to add metrics to.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="serviceName">The name of the service. Defaults to assembly name.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// Configuration example:
    /// {
    ///   "OpenTelemetryMetrics": {
    ///     "Enabled": true,
    ///     "EnableAspNetCoreMetrics": true,
    ///     "EnableHttpClientMetrics": true,
    ///     "EnableRuntimeMetrics": true,
    ///     "OtlpEndpoint": "http://localhost:4317",
    ///     "EnablePrometheusExporter": false,
    ///     "PrometheusEndpoint": "/metrics",
    ///     "ExportIntervalSeconds": 60
    ///   }
    /// }
    /// </remarks>
    public static IServiceCollection AddWanderpoolMetricsWithConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        string? serviceName = null)
    {
        // Configure MetricsConfiguration from appsettings
        services.Configure<MetricsConfiguration>(
            configuration.GetSection(MetricsConfiguration.Name));

        serviceName ??= Assembly.GetCallingAssembly().GetName().Name ?? "UnknownService";
        var serviceVersion = Assembly.GetCallingAssembly().GetName().Version?.ToString() ?? "1.0.0";

        // Get metrics configuration
        var metricsConfig = new MetricsConfiguration();
        configuration.GetSection(MetricsConfiguration.Name).Bind(metricsConfig);

        if (!metricsConfig.Enabled)
        {
            return services;
        }

        services
            .AddOpenTelemetry()
            .WithMetrics(metricsBuilder =>
            {
                var builder = metricsBuilder;

                // Add ASP.NET Core metrics
                if (metricsConfig.EnableAspNetCoreMetrics)
                {
                    builder = builder
                        .AddMeter("OpenTelemetry.Instrumentation.AspNetCore")
                        .AddAspNetCoreInstrumentation();
                }

                // Add HttpClient metrics
                if (metricsConfig.EnableHttpClientMetrics)
                {
                    builder = builder
                        .AddMeter("OpenTelemetry.Instrumentation.Http")
                        .AddHttpClientInstrumentation();
                }

                // Add runtime metrics
                if (metricsConfig.EnableRuntimeMetrics)
                {
                    builder = builder
                        .AddMeter("OpenTelemetry.Instrumentation.Runtime")
                        .AddRuntimeInstrumentation();
                }

                // Configure OTLP exporter
                builder.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(metricsConfig.OtlpEndpoint);
                });
            });

        return services;
    }

    /// <summary>
    /// Adds custom HTTP client metrics instruments to the service collection.
    /// Requires OpenTelemetry metrics to be configured first via AddWanderpoolMetrics().
    /// </summary>
    /// <param name="services">The service collection to add metrics to.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// This creates custom metric instruments for HTTP client tracking:
    /// - wanderpool_http_client_requests_total: Counter for total requests by client, method, and status
    /// - wanderpool_http_client_request_duration_seconds: Histogram for request duration by client, method, and status
    ///
    /// These instruments can be populated by a DelegatingHandler or other HTTP client instrumentation.
    /// </remarks>
    public static IServiceCollection AddWanderpoolHttpClientMetrics(
        this IServiceCollection services)
    {
        var meter = new Meter("Wanderpool.HttpClient", "1.0.0");

        var requestsCounter = meter.CreateCounter<long>(
            "wanderpool_http_client_requests_total",
            description: "Total HTTP client requests");

        var requestDurationHistogram = meter.CreateHistogram<double>(
            "wanderpool_http_client_request_duration_seconds",
            unit: "s",
            description: "HTTP client request duration in seconds");

        var instruments = new HttpClientMetricsInstruments
        {
            RequestsCounter = requestsCounter,
            RequestDurationHistogram = requestDurationHistogram
        };

        services.AddSingleton(instruments);

        return services;
    }

    /// <summary>
    /// Adds circuit breaker state metrics to the service collection.
    /// Requires OpenTelemetry metrics to be configured first via AddWanderpoolMetrics().
    /// </summary>
    /// <param name="services">The service collection to add metrics to.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <remarks>
    /// This creates an observable gauge for circuit breaker state tracking:
    /// - wanderpool_circuit_breaker_state: Observable gauge reporting circuit state (0=Closed, 1=Open, 2=Half-Open)
    /// - Tagged by: circuit_name
    ///
    /// The CircuitBreakerStateRegistry must be updated by resilience pipeline handlers
    /// when circuit state changes occur.
    /// </remarks>
    public static IServiceCollection AddWanderpoolCircuitBreakerMetrics(
        this IServiceCollection services)
    {
        var meter = new Meter("Wanderpool.CircuitBreaker", "1.0.0");
        var stateRegistry = new CircuitBreakerStateRegistry();

        var stateGauge = meter.CreateObservableGauge(
            "wanderpool_circuit_breaker_state",
            () =>
            {
                var measurements = new List<Measurement<int>>();
                foreach (var kvp in stateRegistry.GetAllStates())
                {
                    measurements.Add(new Measurement<int>(
                        kvp.Value,
                        new KeyValuePair<string, object?>("circuit_name", kvp.Key)));
                }
                return measurements;
            },
            description: "Circuit breaker state (0=Closed, 1=Open, 2=Half-Open)");

        var instruments = new CircuitBreakerMetricsInstruments
        {
            CircuitBreakerStateGauge = stateGauge,
            CircuitBreakerStateRegistry = stateRegistry
        };

        services.AddSingleton(instruments);
        services.AddSingleton(stateRegistry);

        return services;
    }
}
