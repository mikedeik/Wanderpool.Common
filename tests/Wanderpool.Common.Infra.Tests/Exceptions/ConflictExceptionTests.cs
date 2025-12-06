using Wanderpool.Common.Infra.Exceptions;

namespace Wanderpool.Common.Infra.Tests.Exceptions;

public class ConflictExceptionTests
{
    [Fact]
    public void Constructor_SetsConflictErrorCode()
    {
        // Arrange & Act
        var exception = new ConflictException("Email already exists.", "user@example.com");

        // Assert
        exception.ErrorCode.Should().Be("CONFLICT");
        exception.ResourceIdentifier.Should().Be("user@example.com");
    }

    // [Fact]
    // public void Exception_IsSerializable()
    // {
    //     // Arrange
    //     var originalException = new ConflictException("Resource already exists.", "res-123");
    //     var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
    //
    //     using var stream = new MemoryStream();
    //
    //     // Act
    //     formatter.Serialize(stream, originalException);
    //     stream.Position = 0;
    //     var deserializedException = (ConflictException)formatter.Deserialize(stream);
    //
    //     // Assert
    //     deserializedException.ErrorCode.Should().Be(originalException.ErrorCode);
    //     deserializedException.ResourceIdentifier.Should().Be(originalException.ResourceIdentifier);
    // }
}
