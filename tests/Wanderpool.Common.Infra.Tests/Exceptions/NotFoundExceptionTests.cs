using Wanderpool.Common.Infra.Exceptions;

namespace Wanderpool.Common.Infra.Tests.Exceptions;

public class NotFoundExceptionTests
{
    [Fact]
    public void Constructor_SetsResourceTypeAndId()
    {
        // Arrange
        var resourceType = "User";
        var resourceId = "123";

        // Act
        var exception = new NotFoundException(resourceType, resourceId);

        // Assert
        Assert.Equal("NOT_FOUND", exception.ErrorCode);
        Assert.Equal(resourceType, exception.ResourceType);
        Assert.Equal(resourceId, exception.ResourceId);
    }

    // [Fact]
    // public void Exception_IsSerializable()
    // {
    //     // Arrange
    //     var originalException = new NotFoundException("User", "123");
    //     var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
    //
    //     using var stream = new MemoryStream();
    //
    //     // Act
    //     formatter.Serialize(stream, originalException);
    //     stream.Position = 0;
    //     var deserializedException = (NotFoundException)formatter.Deserialize(stream);
    //
    //     // Assert
    //     deserializedException.ErrorCode.Should().Be(originalException.ErrorCode);
    //     deserializedException.ResourceType.Should().Be(originalException.ResourceType);
    //     deserializedException.ResourceId.Should().Be(originalException.ResourceId);
    // }

    [Fact]
    public void Message_IncludesResourceDetails()
    {
        // Arrange & Act
        var exception = new NotFoundException("User", "456");

        // Assert
        Assert.Contains("User", exception.Message);
        Assert.Contains("456", exception.Message);
    }
}
