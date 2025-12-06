using Wanderpool.Common.Infra.Exceptions;

namespace Wanderpool.Common.Infra.Tests.Exceptions;

public class BusinessRuleExceptionTests
{
    [Fact]
    public void Constructor_SetsBusiness_RuleErrorCode()
    {
        // Arrange & Act
        var exception = new BusinessRuleException("OrderQuantity", "Order quantity cannot exceed inventory.");

        // Assert
        Assert.Equal("BUSINESS_RULE_VIOLATION", exception.ErrorCode);
        Assert.Equal("OrderQuantity", exception.RuleName);
    }

    // [Fact]
    // public void Exception_IsSerializable()
    // {
    //     // Arrange
    //     var originalException = new BusinessRuleException("MinimumAge", "Minimum age requirement not met.");
    //     var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
    //
    //     using var stream = new MemoryStream();
    //
    //     // Act
    //     formatter.Serialize(stream, originalException);
    //     stream.Position = 0;
    //     var deserializedException = (BusinessRuleException)formatter.Deserialize(stream);
    //
    //     // Assert
    //     deserializedException.ErrorCode.Should().Be(originalException.ErrorCode);
    //     deserializedException.RuleName.Should().Be(originalException.RuleName);
    // }

    [Fact]
    public void Message_IncludesRuleName()
    {
        // Arrange & Act
        var exception = new BusinessRuleException("InvalidStatus", "Status transition not allowed.");

        // Assert
        Assert.Contains("InvalidStatus", exception.Message);
    }
}
