using Wanderpool.Common.Infra.Logging;

namespace Wanderpool.Common.Infra.Tests.Logging;

/// <summary>
/// Tests for RequestLoggingOptions configuration class.
/// </summary>
public class RequestLoggingOptionsTests
{
    [Fact]
    public void DefaultOptions_BodyLoggingDisabled()
    {
        // Arrange & Act
        var options = new RequestLoggingOptions();

        // Assert
        Assert.False(options.EnableRequestBodyLogging);
        Assert.False(options.EnableResponseBodyLogging);
    }

    [Fact]
    public void DefaultOptions_SensitiveHeadersPopulated()
    {
        // Arrange & Act
        var options = new RequestLoggingOptions();

        // Assert
        Assert.NotNull(options.SensitiveHeaders);
        Assert.NotEmpty(options.SensitiveHeaders);
        Assert.Contains("Authorization", options.SensitiveHeaders);
        Assert.Contains("X-Api-Key", options.SensitiveHeaders);
        Assert.Contains("Cookie", options.SensitiveHeaders);
    }

    [Fact]
    public void Options_AreModifiable()
    {
        // Arrange
        var options = new RequestLoggingOptions();

        // Act
        options.EnableRequestBodyLogging = true;
        options.EnableResponseBodyLogging = true;
        options.MaxBodySizeLogged = 1024;
        options.SensitiveHeaders.Clear();
        options.SensitiveHeaders.Add("CustomHeader");

        // Assert
        Assert.True(options.EnableRequestBodyLogging);
        Assert.True(options.EnableResponseBodyLogging);
        Assert.Equal(1024, options.MaxBodySizeLogged);
        Assert.Single(options.SensitiveHeaders);
        Assert.Contains("CustomHeader", options.SensitiveHeaders);
    }

    [Fact]
    public void DefaultOptions_MaxBodySizeIsConfigurable()
    {
        // Arrange & Act
        var options = new RequestLoggingOptions();

        // Assert
        Assert.True(options.MaxBodySizeLogged > 0);
    }

    [Fact]
    public void DefaultOptions_IncludeQueryStringIsConfigurable()
    {
        // Arrange & Act
        var options = new RequestLoggingOptions();

        // Assert - default should be true (log query strings)
        Assert.True(options.IncludeQueryString);
    }
}
