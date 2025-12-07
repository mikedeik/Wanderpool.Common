using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Wanderpool.Common.Infra.Policies;

namespace Wanderpool.Common.Infra.Tests.Policies;

/// <summary>
/// Tests for loading named resilience pipeline configurations from appsettings.json.
/// Validates that pipelines are properly registered from configuration.
/// </summary>
public class ResiliencePipelineConfigurationTests
{
    /// <summary>
    /// Test: AddWanderpoolNamedResiliencePipelines loads pipelines from configuration section.
    /// </summary>
    [Fact]
    public void AddWanderpoolNamedResiliencePipelines_LoadsPipelinesFromConfiguration()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            { "Resilience:Pipelines:FastAPI:Timeout:TimeoutSeconds", "5" },
            { "Resilience:Pipelines:FastAPI:Retry:MaxRetryAttempts", "1" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolNamedResiliencePipelines(configuration);
        var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<ResiliencePipelineRegistry>();

        // Assert - registry is registered
        Assert.NotNull(registry);
    }

    /// <summary>
    /// Test: Multiple named pipelines are loaded from configuration.
    /// </summary>
    [Fact]
    public void AddWanderpoolNamedResiliencePipelines_LoadsMultiplePipelines()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            { "Resilience:Pipelines:FastAPI:Timeout:TimeoutSeconds", "5" },
            { "Resilience:Pipelines:FastAPI:Retry:MaxRetryAttempts", "1" },
            { "Resilience:Pipelines:SlowService:Timeout:TimeoutSeconds", "30" },
            { "Resilience:Pipelines:SlowService:Retry:MaxRetryAttempts", "5" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolNamedResiliencePipelines(configuration);
        var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<ResiliencePipelineRegistry>();

        // Assert - both pipelines are registered
        Assert.True(registry.Contains("FastAPI"));
        Assert.True(registry.Contains("SlowService"));
    }

    /// <summary>
    /// Test: Each named pipeline has its own independent settings.
    /// </summary>
    [Fact]
    public void AddWanderpoolNamedResiliencePipelines_EachPipelineHasOwnSettings()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            { "Resilience:Pipelines:FastAPI:Timeout:TimeoutSeconds", "5" },
            { "Resilience:Pipelines:FastAPI:Retry:MaxRetryAttempts", "1" },
            { "Resilience:Pipelines:SlowService:Timeout:TimeoutSeconds", "30" },
            { "Resilience:Pipelines:SlowService:Retry:MaxRetryAttempts", "5" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolNamedResiliencePipelines(configuration);
        var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<ResiliencePipelineRegistry>();

        var fastAPI = registry.Get("FastAPI");
        var slowService = registry.Get("SlowService");

        // Assert - each has different settings
        Assert.Equal(5, fastAPI.Timeout.TimeoutSeconds);
        Assert.Equal(1, fastAPI.Retry.MaxRetryAttempts);
        Assert.Equal(30, slowService.Timeout.TimeoutSeconds);
        Assert.Equal(5, slowService.Retry.MaxRetryAttempts);
    }

    /// <summary>
    /// Test: Configuration with sensible settings validates successfully.
    /// </summary>
    [Fact]
    public void AddWanderpoolNamedResiliencePipelines_WithValidConfiguration_LoadsSuccessfully()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            { "Resilience:Pipelines:ValidPipeline:Timeout:TimeoutSeconds", "15" },
            { "Resilience:Pipelines:ValidPipeline:Retry:MaxRetryAttempts", "3" },
            { "Resilience:Pipelines:ValidPipeline:CircuitBreaker:FailureRatio", "0.5" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolNamedResiliencePipelines(configuration);
        var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<ResiliencePipelineRegistry>();
        var pipeline = registry.Get("ValidPipeline");

        // Assert
        Assert.Equal(15, pipeline.Timeout.TimeoutSeconds);
        Assert.Equal(3, pipeline.Retry.MaxRetryAttempts);
        Assert.Equal(0.5, pipeline.CircuitBreaker.FailureRatio);
    }

    /// <summary>
    /// Test: Registry is registered as singleton in dependency injection.
    /// </summary>
    [Fact]
    public void AddWanderpoolNamedResiliencePipelines_RegistersRegistryAsSingleton()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>())
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolNamedResiliencePipelines(configuration);
        var provider = services.BuildServiceProvider();
        var registry1 = provider.GetRequiredService<ResiliencePipelineRegistry>();
        var registry2 = provider.GetRequiredService<ResiliencePipelineRegistry>();

        // Assert - same instance (singleton)
        Assert.Same(registry1, registry2);
    }

    /// <summary>
    /// Test: Empty configuration section doesn't throw, just creates empty registry.
    /// </summary>
    [Fact]
    public void AddWanderpoolNamedResiliencePipelines_WithEmptyConfiguration_CreatesEmptyRegistry()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>())
            .Build();

        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolNamedResiliencePipelines(configuration);
        var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<ResiliencePipelineRegistry>();

        // Assert - registry exists but is empty
        Assert.NotNull(registry);
        Assert.Equal(0, registry.Count);
    }
}
