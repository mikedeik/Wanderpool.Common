using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Wanderpool.Common.Infra.Policies;

/// <summary>
/// Extension methods for registering named resilience pipelines from configuration.
/// Loads pipeline configurations from appsettings.json "Resilience:Pipelines" section.
/// </summary>
public static class ResilienceConfigurationExtensions
{
    /// <summary>
    /// Adds named resilience pipelines from appsettings.json configuration.
    /// Each pipeline configuration under "Resilience:Pipelines:{PipelineName}" is registered.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or configuration is null.</exception>
    /// <example>
    /// appsettings.json:
    /// {
    ///   "Resilience": {
    ///     "Pipelines": {
    ///       "FastAPI": {
    ///         "Timeout": { "TimeoutSeconds": 5 },
    ///         "Retry": { "MaxRetryAttempts": 1 }
    ///       },
    ///       "SlowService": {
    ///         "Timeout": { "TimeoutSeconds": 30 },
    ///         "Retry": { "MaxRetryAttempts": 5 }
    ///       }
    ///     }
    ///   }
    /// }
    /// </example>
    public static IServiceCollection AddWanderpoolNamedResiliencePipelines(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Register the registry as a singleton with a factory that loads from configuration
        services.AddSingleton<ResiliencePipelineRegistry>(provider =>
        {
            var registry = new ResiliencePipelineRegistry();

            // Get the Resilience:Pipelines section
            var pipelinesSection = configuration.GetSection("Resilience:Pipelines");

            if (pipelinesSection.Exists())
            {
                foreach (var pipelineSection in pipelinesSection.GetChildren())
                {
                    var pipelineName = pipelineSection.Key;

                    try
                    {
                        // Bind the configuration to ResilienceOptions
                        var options = new ResilienceOptions();
                        pipelineSection.Bind(options);

                        // Validate the configuration
                        ValidateResilienceOptions(pipelineName, options);

                        // Register the pipeline
                        registry.Register(pipelineName, options);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            $"Failed to load resilience pipeline '{pipelineName}' from configuration. " +
                            $"Ensure the 'Resilience:Pipelines:{pipelineName}' section is correctly configured.",
                            ex);
                    }
                }
            }

            return registry;
        });

        return services;
    }

    /// <summary>
    /// Validates resilience options configuration.
    /// </summary>
    /// <param name="pipelineName">The name of the pipeline being validated.</param>
    /// <param name="options">The resilience options to validate.</param>
    /// <exception cref="ArgumentException">Thrown when validation fails.</exception>
    private static void ValidateResilienceOptions(string pipelineName, ResilienceOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var errors = new List<string>();

        // Validate Timeout
        if (options.Timeout.TimeoutSeconds <= 0)
        {
            errors.Add($"Timeout.TimeoutSeconds must be positive (got {options.Timeout.TimeoutSeconds})");
        }

        // Validate Retry
        if (options.Retry.MaxRetryAttempts < 0)
        {
            errors.Add($"Retry.MaxRetryAttempts must be non-negative (got {options.Retry.MaxRetryAttempts})");
        }

        if (options.Retry.InitialDelayMilliseconds < 0)
        {
            errors.Add($"Retry.InitialDelayMilliseconds must be non-negative (got {options.Retry.InitialDelayMilliseconds})");
        }

        if (options.Retry.MaxDelayMilliseconds < options.Retry.InitialDelayMilliseconds)
        {
            errors.Add($"Retry.MaxDelayMilliseconds ({options.Retry.MaxDelayMilliseconds}) must be >= InitialDelayMilliseconds ({options.Retry.InitialDelayMilliseconds})");
        }

        // Validate Circuit Breaker
        if (options.CircuitBreaker.FailureRatio < 0 || options.CircuitBreaker.FailureRatio > 1)
        {
            errors.Add($"CircuitBreaker.FailureRatio must be between 0 and 1 (got {options.CircuitBreaker.FailureRatio})");
        }

        if (options.CircuitBreaker.MinimumThroughput <= 0)
        {
            errors.Add($"CircuitBreaker.MinimumThroughput must be positive (got {options.CircuitBreaker.MinimumThroughput})");
        }

        if (options.CircuitBreaker.SamplingPeriodSeconds <= 0)
        {
            errors.Add($"CircuitBreaker.SamplingPeriodSeconds must be positive (got {options.CircuitBreaker.SamplingPeriodSeconds})");
        }

        if (options.CircuitBreaker.BreakDurationSeconds <= 0)
        {
            errors.Add($"CircuitBreaker.BreakDurationSeconds must be positive (got {options.CircuitBreaker.BreakDurationSeconds})");
        }

        // Validate Hedging
        if (options.Hedging.DelayMilliseconds < 0)
        {
            errors.Add($"Hedging.DelayMilliseconds must be non-negative (got {options.Hedging.DelayMilliseconds})");
        }

        if (options.Hedging.MaxHedgedAttempts < 0)
        {
            errors.Add($"Hedging.MaxHedgedAttempts must be non-negative (got {options.Hedging.MaxHedgedAttempts})");
        }

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                $"Invalid configuration for resilience pipeline '{pipelineName}':\n" +
                string.Join("\n", errors.Select(e => $"  - {e}")));
        }
    }
}
