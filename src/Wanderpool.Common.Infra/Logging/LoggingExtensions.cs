using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace Wanderpool.Common.Infra.Logging;

/// <summary>
/// Extension methods for configuring structured logging with Serilog.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Adds Wanderpool structured logging configuration using Serilog.
    /// Configures JSON formatting for production and readable format for development.
    /// Includes standard enrichers for context information.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder to configure logging for.</param>
    /// <param name="serviceName">The name of the service for log enrichment.</param>
    /// <returns>The WebApplicationBuilder for method chaining.</returns>
    /// <remarks>
    /// Configuration hierarchy (highest to lowest priority):
    /// 1. Environment-specific settings (appsettings.{Environment}.json)
    /// 2. Base settings (appsettings.json)
    /// 3. Default Wanderpool configuration
    ///
    /// Example appsettings.json configuration:
    /// {
    ///   "Serilog": {
    ///     "MinimumLevel": "Information",
    ///     "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"],
    ///     "WriteTo": [
    ///       { "Name": "Console" },
    ///       { "Name": "File", "Args": { "path": "logs/app.log" } }
    ///     ]
    ///   }
    /// }
    /// </remarks>
    public static WebApplicationBuilder AddWanderpoolLogging(this WebApplicationBuilder builder, string? serviceName = null)
    {
        serviceName ??= builder.Environment.ApplicationName;

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            // Standard enrichers
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("ServiceName", serviceName)
            // Environment-specific configuration
            .CreateBootstrapLogger();

        builder.Host.UseSerilog((context, serviceProvider, loggerConfig) =>
        {
            loggerConfig
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                // Standard enrichers
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProperty("ServiceName", serviceName);

            // Configure output based on environment
            if (context.HostingEnvironment.IsDevelopment())
            {
                loggerConfig.WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
            }
            else
            {
                loggerConfig.WriteTo.Console(
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
            }

            // Support configuration from appsettings.json
            loggerConfig.ReadFrom.Configuration(context.Configuration);
        });

        return builder;
    }
}
