using Wanderpool.Common.Infra.Exceptions;

namespace Wanderpool.Common.Infra.Tests.Exceptions;

public class ValidationExceptionTests
{
    [Fact]
    public void Constructor_StoresValidationErrors()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Email is required" } },
            { "Name", new[] { "Name is required", "Name must be at least 3 characters" } }
        };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        exception.Errors.Should().Equal(errors);
        exception.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenErrorsIsEmpty()
    {
        // Arrange
        var emptyErrors = new Dictionary<string, string[]>();

        // Act & Assert
        var act = () => new ValidationException(emptyErrors);
        act.Should().Throw<ArgumentException>().WithParameterName("errors");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenErrorsIsNull()
    {
        // Act & Assert
        var act = () => new ValidationException(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("errors");
    }

    [Fact]
    public void ErrorCode_DefaultsToValidationError()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Field", new[] { "Error" } }
        };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        exception.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    // [Fact]
    // public void Exception_IsSerializable()
    // {
    //     // Arrange
    //     var errors = new Dictionary<string, string[]>
    //     {
    //         { "Email", new[] { "Email is required" } }
    //     };
    //     var originalException = new ValidationException(errors);
    //     var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
    //
    //     using var stream = new MemoryStream();
    //
    //     // Act
    //     formatter.Serialize(stream, originalException);
    //     stream.Position = 0;
    //     var deserializedException = (ValidationException)formatter.Deserialize(stream);
    //
    //     // Assert
    //     deserializedException.ErrorCode.Should().Be(originalException.ErrorCode);
    //     deserializedException.Errors.Should().Equal(originalException.Errors);
    // }

    [Fact]
    public void ErrorsProperty_IsImmutable()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Field", new[] { "Error" } }
        };
        var exception = new ValidationException(errors);

        // Act & Assert
        // Verify that Errors property is read-only
        var properties = typeof(ValidationException).GetProperties();
        var errorsProperty = properties.FirstOrDefault(p => p.Name == "Errors");
        errorsProperty.Should().NotBeNull();
        errorsProperty!.CanWrite.Should().BeFalse();
    }

    [Fact]
    public void Message_FormatsErrorsSummary()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Email is required", "Email format is invalid" } },
            { "Name", new[] { "Name is required" } }
        };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        exception.Message.Should().Contain("validation");
        exception.Message.Should().NotBeNull();
        exception.Message.Length.Should().BeGreaterThan(0);
    }
}
