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
        exception.ErrorCode.Should().Be(errorCode);
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenErrorCodeIsNull()
    {
        // Act & Assert
        var act = () => new WanderpoolException(null!, "message");
        act.Should().Throw<ArgumentNullException>().WithParameterName("errorCode");
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenErrorCodeIsEmpty()
    {
        // Act & Assert
        var act = () => new WanderpoolException(string.Empty, "message");
        act.Should().Throw<ArgumentException>();
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
        exception.InnerException.Should().Be(innerException);
        exception.ErrorCode.Should().Be(errorCode);
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void ErrorCode_Property_IsImmutable()
    {
        // Arrange
        var exception = new WanderpoolException("ORIGINAL_CODE", "message");

        // Act & Assert
        // This test verifies that ErrorCode is read-only (immutable)
        var errorCode = exception.ErrorCode;
        errorCode.Should().Be("ORIGINAL_CODE");

        // Should not be able to set it
        var properties = typeof(WanderpoolException).GetProperties();
        var errorCodeProperty = properties.FirstOrDefault(p => p.Name == "ErrorCode");
        errorCodeProperty.Should().NotBeNull();
        errorCodeProperty!.CanWrite.Should().BeFalse();
    }
}
