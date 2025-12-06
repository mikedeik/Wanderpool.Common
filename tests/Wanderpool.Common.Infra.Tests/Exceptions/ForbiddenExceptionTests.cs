using Wanderpool.Common.Infra.Exceptions;

namespace Wanderpool.Common.Infra.Tests.Exceptions;

public class ForbiddenExceptionTests
{
    [Fact]
    public void Constructor_SetsForbiddenErrorCode()
    {
        // Arrange & Act
        var exception = new ForbiddenException("Access to this resource is denied.");

        // Assert
        exception.ErrorCode.Should().Be("FORBIDDEN");
        exception.Message.Should().Contain("Access");
    }

    [Fact]
    public void Exception_IsSerializable()
    {
        // Arrange
        var originalException = new ForbiddenException("Insufficient permissions.");
        var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

        using var stream = new MemoryStream();

        // Act
        formatter.Serialize(stream, originalException);
        stream.Position = 0;
        var deserializedException = (ForbiddenException)formatter.Deserialize(stream);

        // Assert
        deserializedException.ErrorCode.Should().Be(originalException.ErrorCode);
        deserializedException.Message.Should().Be(originalException.Message);
    }
}
