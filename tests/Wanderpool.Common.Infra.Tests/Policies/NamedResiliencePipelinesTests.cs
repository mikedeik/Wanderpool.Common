using Xunit;
using Wanderpool.Common.Infra.Policies;

namespace Wanderpool.Common.Infra.Tests.Policies;

/// <summary>
/// Tests for named resilience pipeline registry.
/// Validates that pipelines can be registered and retrieved by name.
/// </summary>
public class NamedResiliencePipelinesTests
{
    /// <summary>
    /// Test: Pipeline can be registered with a name.
    /// </summary>
    [Fact]
    public void Register_WithNameAndOptions_SuccessfullyRegisters()
    {
        // Arrange
        var registry = new ResiliencePipelineRegistry();
        var options = new ResilienceOptions();
        var pipelineName = "TestPipeline";

        // Act
        registry.Register(pipelineName, options);

        // Assert - no exception thrown
        Assert.NotNull(registry);
    }

    /// <summary>
    /// Test: Registered pipeline can be retrieved by name.
    /// </summary>
    [Fact]
    public void Get_WithRegisteredName_ReturnsRegisteredOptions()
    {
        // Arrange
        var registry = new ResiliencePipelineRegistry();
        var options = new ResilienceOptions
        {
            Timeout = new ResilienceOptions.TimeoutPolicyOptions { TimeoutSeconds = 25 }
        };
        var pipelineName = "TestPipeline";

        // Act
        registry.Register(pipelineName, options);
        var retrieved = registry.Get(pipelineName);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(25, retrieved.Timeout.TimeoutSeconds);
    }

    /// <summary>
    /// Test: Multiple named pipelines can coexist with different configurations.
    /// </summary>
    [Fact]
    public void Register_MultipleNamedPipelines_AllAreStored()
    {
        // Arrange
        var registry = new ResiliencePipelineRegistry();
        var options1 = new ResilienceOptions
        {
            Timeout = new ResilienceOptions.TimeoutPolicyOptions { TimeoutSeconds = 10 }
        };
        var options2 = new ResilienceOptions
        {
            Timeout = new ResilienceOptions.TimeoutPolicyOptions { TimeoutSeconds = 20 }
        };
        var options3 = new ResilienceOptions
        {
            Timeout = new ResilienceOptions.TimeoutPolicyOptions { TimeoutSeconds = 30 }
        };

        // Act
        registry.Register("FastService", options1);
        registry.Register("NormalService", options2);
        registry.Register("SlowService", options3);

        var retrieved1 = registry.Get("FastService");
        var retrieved2 = registry.Get("NormalService");
        var retrieved3 = registry.Get("SlowService");

        // Assert
        Assert.Equal(10, retrieved1.Timeout.TimeoutSeconds);
        Assert.Equal(20, retrieved2.Timeout.TimeoutSeconds);
        Assert.Equal(30, retrieved3.Timeout.TimeoutSeconds);
    }

    /// <summary>
    /// Test: Retrieving an unregistered pipeline name throws exception.
    /// </summary>
    [Fact]
    public void Get_WithUnregisteredName_ThrowsKeyNotFoundException()
    {
        // Arrange
        var registry = new ResiliencePipelineRegistry();
        var pipelineName = "NonExistentPipeline";

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() => registry.Get(pipelineName));
    }

    /// <summary>
    /// Test: Pipelines are stored by reference and are reusable (not cloned).
    /// </summary>
    [Fact]
    public void Register_AndRetrieve_ReturnsSamePipelineReference()
    {
        // Arrange
        var registry = new ResiliencePipelineRegistry();
        var options = new ResilienceOptions();
        var pipelineName = "ReusablePipeline";

        // Act
        registry.Register(pipelineName, options);
        var retrieved1 = registry.Get(pipelineName);
        var retrieved2 = registry.Get(pipelineName);

        // Assert - same reference
        Assert.Same(options, retrieved1);
        Assert.Same(retrieved1, retrieved2);
    }

    /// <summary>
    /// Test: Registering with null name throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void Register_WithNullName_ThrowsArgumentNullException()
    {
        // Arrange
        var registry = new ResiliencePipelineRegistry();
        var options = new ResilienceOptions();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => registry.Register(null!, options));
    }

    /// <summary>
    /// Test: Registering with null options throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void Register_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var registry = new ResiliencePipelineRegistry();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => registry.Register("TestPipeline", null!));
    }

    /// <summary>
    /// Test: Registering duplicate name overwrites previous registration.
    /// </summary>
    [Fact]
    public void Register_WithDuplicateName_OverwritesPreviousRegistration()
    {
        // Arrange
        var registry = new ResiliencePipelineRegistry();
        var options1 = new ResilienceOptions
        {
            Timeout = new ResilienceOptions.TimeoutPolicyOptions { TimeoutSeconds = 10 }
        };
        var options2 = new ResilienceOptions
        {
            Timeout = new ResilienceOptions.TimeoutPolicyOptions { TimeoutSeconds = 20 }
        };
        var pipelineName = "DuplicatePipeline";

        // Act
        registry.Register(pipelineName, options1);
        registry.Register(pipelineName, options2);
        var retrieved = registry.Get(pipelineName);

        // Assert
        Assert.Equal(20, retrieved.Timeout.TimeoutSeconds);
    }

    /// <summary>
    /// Test: TryGet returns false for unregistered pipelines without throwing.
    /// </summary>
    [Fact]
    public void TryGet_WithUnregisteredName_ReturnsFalseAndNullOptions()
    {
        // Arrange
        var registry = new ResiliencePipelineRegistry();
        var pipelineName = "NonExistentPipeline";

        // Act
        var success = registry.TryGet(pipelineName, out var options);

        // Assert
        Assert.False(success);
        Assert.Null(options);
    }

    /// <summary>
    /// Test: TryGet returns true and options for registered pipelines.
    /// </summary>
    [Fact]
    public void TryGet_WithRegisteredName_ReturnsTrueAndOptions()
    {
        // Arrange
        var registry = new ResiliencePipelineRegistry();
        var originalOptions = new ResilienceOptions
        {
            Timeout = new ResilienceOptions.TimeoutPolicyOptions { TimeoutSeconds = 15 }
        };
        var pipelineName = "TestPipeline";

        // Act
        registry.Register(pipelineName, originalOptions);
        var success = registry.TryGet(pipelineName, out var retrievedOptions);

        // Assert
        Assert.True(success);
        Assert.NotNull(retrievedOptions);
        Assert.Equal(15, retrievedOptions.Timeout.TimeoutSeconds);
    }
}
