using Wanderpool.Common.Infra;
using Xunit;

namespace Wanderpool.Common.Infra.Tests;

/// <summary>
/// Tests for configuration validation in Wanderpool infrastructure.
/// </summary>
public class ConfigurationValidationTests
{
    /// <summary>
    /// Test: Missing required ServiceName throws exception.
    /// </summary>
    [Fact]
    public void Configuration_MissingServiceName_ThrowsArgumentException()
    {
        // Arrange
        var options = new WanderpoolOptions
        {
            ServiceName = string.Empty
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            WanderpoolConfigurationValidator.Validate(options);
        });
    }

    /// <summary>
    /// Test: Missing required ServiceVersion throws exception.
    /// </summary>
    [Fact]
    public void Configuration_MissingServiceVersion_ThrowsArgumentException()
    {
        // Arrange
        var options = new WanderpoolOptions
        {
            ServiceName = "TestService",
            ServiceVersion = string.Empty
        };

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            WanderpoolConfigurationValidator.Validate(options);
        });
    }

    /// <summary>
    /// Test: Valid configuration passes validation.
    /// </summary>
    [Fact]
    public void Configuration_ValidConfiguration_Passes()
    {
        // Arrange
        var options = new WanderpoolOptions
        {
            ServiceName = "TestService",
            ServiceVersion = "1.0.0"
        };

        // Act & Assert - should not throw
        WanderpoolConfigurationValidator.Validate(options);
    }

    /// <summary>
    /// Test: Error messages are helpful.
    /// </summary>
    [Fact]
    public void Configuration_ValidationError_HasHelpfulMessage()
    {
        // Arrange
        var options = new WanderpoolOptions
        {
            ServiceName = string.Empty
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
        {
            WanderpoolConfigurationValidator.Validate(options);
        });

        Assert.NotNull(exception.Message);
        Assert.NotEmpty(exception.Message);
        Assert.Contains("ServiceName", exception.Message);
    }
}
