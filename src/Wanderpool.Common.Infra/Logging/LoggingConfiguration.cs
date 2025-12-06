namespace Wanderpool.Common.Infra.Logging;

/// <summary>
/// Configuration for structured logging with Serilog.
/// Maps to appsettings.json "Logging" section.
/// </summary>
public class LoggingConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string Name = "Logging";

    /// <summary>
    /// Minimum log level for the root logger.
    /// Valid values: Verbose, Debug, Information, Warning, Error, Fatal
    /// Default: Information
    /// </summary>
    public string MinimumLevel { get; set; } = "Information";

    /// <summary>
    /// Log level overrides for specific namespaces.
    /// </summary>
    public LogLevelOverridesConfig LogLevelOverrides { get; set; } = new();

    /// <summary>
    /// Output templates for different environments.
    /// </summary>
    public OutputTemplatesConfig OutputTemplates { get; set; } = new();

    /// <summary>
    /// Whether to enrich logs with context information.
    /// </summary>
    public EnrichmentConfig Enrichment { get; set; } = new();

    /// <summary>
    /// Log level override configuration for specific namespaces.
    /// </summary>
    public class LogLevelOverridesConfig
    {
        /// <summary>
        /// Log level for Microsoft.* namespace.
        /// Default: Warning (reduces verbose ASP.NET Core framework logs)
        /// </summary>
        public string Microsoft { get; set; } = "Warning";

        /// <summary>
        /// Log level for Microsoft.Hosting.Lifetime namespace.
        /// Default: Information (shows server startup/shutdown events)
        /// </summary>
        public string MicrosoftHostingLifetime { get; set; } = "Information";
    }

    /// <summary>
    /// Output template configuration for formatted log messages.
    /// </summary>
    public class OutputTemplatesConfig
    {
        /// <summary>
        /// Log output template for development environment.
        /// Example: "[HH:mm:ss INF] Logger: Message"
        /// Default: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
        /// </summary>
        public string Development { get; set; } = "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";

        /// <summary>
        /// Log output template for production environment.
        /// Example: "2024-01-15 14:30:45.123 +00:00 [INF] Logger: Message"
        /// Default: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
        /// </summary>
        public string Production { get; set; } = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";
    }

    /// <summary>
    /// Enrichment configuration for adding contextual information to logs.
    /// </summary>
    public class EnrichmentConfig
    {
        /// <summary>
        /// Whether to include machine name in logs.
        /// Default: true
        /// </summary>
        public bool WithMachineName { get; set; } = true;

        /// <summary>
        /// Whether to include process ID in logs.
        /// Default: true
        /// </summary>
        public bool WithProcessId { get; set; } = true;

        /// <summary>
        /// Whether to include thread ID in logs.
        /// Default: true
        /// </summary>
        public bool WithThreadId { get; set; } = true;

        /// <summary>
        /// Whether to include environment name in logs.
        /// Default: true
        /// </summary>
        public bool WithEnvironmentName { get; set; } = true;

        /// <summary>
        /// Whether to use log context properties.
        /// Default: true
        /// </summary>
        public bool FromLogContext { get; set; } = true;
    }
}
