using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wanderpool.Common.Infra;
using Wanderpool.Common.Infra.Exceptions;
using Wanderpool.Common.Infra.Telemetry;

namespace Wanderpool.Common.Infra.TestHelpers;

/// <summary>
/// Builder for creating TestServer instances with Wanderpool infrastructure.
/// Provides a fluent API for integration testing setup.
/// </summary>
public class WanderpoolTestServerBuilder
{
    private string? _serviceName;
    private string _serviceVersion = "1.0.0";
    private Dictionary<string, string?> _configuration = new();
    private readonly List<Action<IServiceCollection>> _serviceConfigurations = new();
    private readonly List<(string Path, RequestDelegate Handler)> _endpoints = new();

    /// <summary>
    /// Sets the service name for telemetry and logging.
    /// </summary>
    /// <param name="serviceName">The service name.</param>
    /// <returns>The builder for chaining.</returns>
    public WanderpoolTestServerBuilder WithServiceName(string serviceName)
    {
        _serviceName = serviceName;
        return this;
    }

    /// <summary>
    /// Sets the service version for telemetry.
    /// </summary>
    /// <param name="serviceVersion">The service version.</param>
    /// <returns>The builder for chaining.</returns>
    public WanderpoolTestServerBuilder WithServiceVersion(string serviceVersion)
    {
        _serviceVersion = serviceVersion;
        return this;
    }

    /// <summary>
    /// Adds custom configuration settings.
    /// </summary>
    /// <param name="configuration">Dictionary of configuration key-value pairs.</param>
    /// <returns>The builder for chaining.</returns>
    public WanderpoolTestServerBuilder WithConfiguration(Dictionary<string, string?> configuration)
    {
        foreach (var kvp in configuration)
        {
            _configuration[kvp.Key] = kvp.Value;
        }
        return this;
    }

    /// <summary>
    /// Configures services with custom registrations or overrides.
    /// </summary>
    /// <param name="configure">Action to configure services.</param>
    /// <returns>The builder for chaining.</returns>
    public WanderpoolTestServerBuilder ConfigureServices(Action<IServiceCollection> configure)
    {
        _serviceConfigurations.Add(configure);
        return this;
    }

    /// <summary>
    /// Adds a custom endpoint for testing.
    /// </summary>
    /// <param name="path">The endpoint path.</param>
    /// <param name="handler">The request handler delegate.</param>
    /// <returns>The builder for chaining.</returns>
    public WanderpoolTestServerBuilder WithEndpoint(string path, RequestDelegate handler)
    {
        _endpoints.Add((path, handler));
        return this;
    }

    /// <summary>
    /// Builds the TestServer with all configured options.
    /// </summary>
    /// <returns>A configured TestServer instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when service name is not set.</exception>
    public TestServer Build()
    {
        if (string.IsNullOrWhiteSpace(_serviceName))
        {
            throw new InvalidOperationException(
                "Service name is required. Call WithServiceName() before Build().");
        }

        var hostBuilder = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();

                // Configure configuration
                webBuilder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(_configuration);
                });

                // Configure services
                webBuilder.ConfigureServices((context, services) =>
                {
                    // Add Wanderpool infrastructure with test-appropriate settings
                    services.AddWanderpoolInfrastructure(options =>
                    {
                        options.ServiceName = _serviceName!;
                        options.ServiceVersion = _serviceVersion;
                        // Disable telemetry exporters for testing
                        options.EnableTracing = false;
                        options.EnableMetrics = false;
                    });

                    // Apply custom service configurations
                    foreach (var configure in _serviceConfigurations)
                    {
                        configure(services);
                    }

                    services.AddRouting();
                });

                // Configure application
                webBuilder.Configure(app =>
                {
                    // Add Wanderpool middleware
                    app.UseRouting();
                    app.UseMiddleware<GlobalExceptionMiddleware>();
                    app.UseMiddleware<CorrelationIdMiddleware>();

                    app.UseEndpoints(endpoints =>
                    {
                        // Map health endpoints
                        endpoints.MapGet("/health/live", () => Results.Ok(new { status = "Healthy" }));
                        endpoints.MapGet("/health/ready", () => Results.Ok(new { status = "Ready" }));

                        // Map custom test endpoints
                        foreach (var (path, handler) in _endpoints)
                        {
                            endpoints.MapGet(path, handler);
                        }
                    });
                });
            });

        var host = hostBuilder.Build();
        host.Start();

        return host.GetTestServer();
    }
}
