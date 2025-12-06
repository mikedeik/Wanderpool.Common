using System.Runtime.Serialization;
using Wanderpool.Common.Infra.Exceptions;

namespace Wanderpool.Common.Infra.Tests.Exceptions;

public class WanderpoolExceptionTests
{
    [Fact]
    public void Constructor_SetsErrorCodeAndMessage()
    {
        // Arrange
        var errorCode = "TEST_ERROR";
        var message = "Test error message";

        // Act
        var exception = new WanderpoolException(errorCode, message);

        // Assert
        Assert.Equal(errorCode, exception.ErrorCode);
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenErrorCodeIsNull()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new WanderpoolException(null!, "message"));
        Assert.NotNull(exception);
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenErrorCodeIsEmpty()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new WanderpoolException(string.Empty, "message"));
        Assert.NotNull(exception);
    }

    // [Fact]
    // public void Exception_IsSerializable()
    // {
    //     // Arrange
    //     var originalException = new WanderpoolException("TEST_ERROR", "Test message");
    //     var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
    //
    //     using var stream = new MemoryStream();
    //
    //     // Act
    //     formatter.Serialize(stream, originalException);
    //     stream.Position = 0;
    //     var deserializedException = (WanderpoolException)formatter.Deserialize(stream);
    //
    //     // Assert
    //     deserializedException.ErrorCode.Should().Be(originalException.ErrorCode);
    //     deserializedException.Message.Should().Be(originalException.Message);
    // }

    [Fact]
    public void Exception_PreservesInnerException()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner error");
        var errorCode = "TEST_ERROR";
        var message = "Outer error";

        // Act
        var exception = new WanderpoolException(errorCode, message, innerException);

        // Assert
        Assert.Equal(innerException, exception.InnerException);
        Assert.Equal(errorCode, exception.ErrorCode);
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void ErrorCode_Property_IsImmutable()
    {
        // Arrange
        var exception = new WanderpoolException("ORIGINAL_CODE", "message");

        // Act & Assert
        // This test verifies that ErrorCode is read-only (immutable)
        var errorCode = exception.ErrorCode;
        Assert.Equal("ORIGINAL_CODE", errorCode);

        // Should not be able to set it
        var properties = typeof(WanderpoolException).GetProperties();
        var errorCodeProperty = properties.FirstOrDefault(p => p.Name == "ErrorCode");
        Assert.NotNull(errorCodeProperty);
        Assert.False(errorCodeProperty!.CanWrite);
    }
}
