using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
    /// Uses hardcoded defaults. For environment-specific configuration, use
    /// AddWanderpoolLoggingWithConfiguration() instead.
    ///
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
        var config = new LoggingConfiguration();
        return AddWanderpoolLoggingInternal(builder, config, serviceName);
    }

    /// <summary>
    /// Adds Wanderpool structured logging configuration using options from appsettings.json.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder to configure logging for.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="serviceName">The name of the service for log enrichment.</param>
    /// <returns>The WebApplicationBuilder for method chaining.</returns>
    /// <remarks>
    /// Reads configuration from appsettings.json "Logging" section.
    /// This is the recommended approach for production applications.
    /// </remarks>
    public static WebApplicationBuilder AddWanderpoolLoggingWithConfiguration(
        this WebApplicationBuilder builder,
        IConfiguration configuration,
        string? serviceName = null)
    {
        var config = new LoggingConfiguration();
        configuration.GetSection(LoggingConfiguration.Name).Bind(config);
        return AddWanderpoolLoggingInternal(builder, config, serviceName);
    }

    /// <summary>
    /// Internal implementation of logging configuration.
    /// </summary>
    private static WebApplicationBuilder AddWanderpoolLoggingInternal(
        WebApplicationBuilder builder,
        LoggingConfiguration config,
        string? serviceName = null)
    {
        serviceName ??= builder.Environment.ApplicationName;

        // Parse minimum level from configuration
        var minimumLevel = Enum.TryParse<LogEventLevel>(config.MinimumLevel, true, out var level)
            ? level
            : LogEventLevel.Information;

        var microsoftLevel = Enum.TryParse<LogEventLevel>(config.LogLevelOverrides.Microsoft, true, out var mLevel)
            ? mLevel
            : LogEventLevel.Warning;

        var lifetimeLevel = Enum.TryParse<LogEventLevel>(config.LogLevelOverrides.MicrosoftHostingLifetime, true, out var lLevel)
            ? lLevel
            : LogEventLevel.Information;

        // Configure bootstrap logger
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(minimumLevel)
            .MinimumLevel.Override("Microsoft", microsoftLevel)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", lifetimeLevel)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("ServiceName", serviceName)
            .CreateBootstrapLogger();

        // Configure main logger
        builder.Host.UseSerilog((context, serviceProvider, loggerConfig) =>
        {
            loggerConfig
                .MinimumLevel.Is(minimumLevel)
                .MinimumLevel.Override("Microsoft", microsoftLevel)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", lifetimeLevel)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentName()
                .Enrich.WithProperty("ServiceName", serviceName);

            // Configure output based on environment
            string outputTemplate = context.HostingEnvironment.IsDevelopment()
                ? config.OutputTemplates.Development
                : config.OutputTemplates.Production;

            loggerConfig.WriteTo.Console(outputTemplate: outputTemplate);

            // Support configuration from appsettings.json
            loggerConfig.ReadFrom.Configuration(context.Configuration);
        });

        return builder;
    }
}
