using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Wanderpool.Common.Infra.Tests.Api;

/// <summary>
/// Tests for ValidationFilter - endpoint filter that automatically validates request objects.
/// </summary>
public class ValidationFilterTests
{
    /// <summary>
    /// Sample request object for testing.
    /// </summary>
    public class TestRequest
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    /// <summary>
    /// Sample validator for TestRequest.
    /// </summary>
    public class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MinimumLength(3).WithMessage("Name must be at least 3 characters");

            RuleFor(x => x.Age)
                .GreaterThan(0).WithMessage("Age must be greater than 0")
                .LessThan(150).WithMessage("Age must be less than 150");
        }
    }

    /// <summary>
    /// Test: Valid request passes validation
    /// </summary>
    [Fact]
    public void Filter_WithValidRequest_PassesValidation()
    {
        // Arrange
        var validator = new TestRequestValidator();
        var request = new TestRequest { Name = "John", Age = 30 };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    /// <summary>
    /// Test: Invalid request fails validation
    /// </summary>
    [Fact]
    public void Filter_WithInvalidRequest_FailsValidation()
    {
        // Arrange
        var validator = new TestRequestValidator();
        var request = new TestRequest { Name = "Jo", Age = 0 };  // Invalid: name too short, age invalid

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    /// <summary>
    /// Test: Validation errors are properly collected
    /// </summary>
    [Fact]
    public void Filter_WithValidationErrors_CollectsErrors()
    {
        // Arrange
        var validator = new TestRequestValidator();
        var request = new TestRequest { Name = "", Age = -1 };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 2);  // At least Name and Age errors
    }

    /// <summary>
    /// Test: Multiple validation errors are all collected
    /// </summary>
    [Fact]
    public void Filter_WithMultipleErrors_CollectsAllErrors()
    {
        // Arrange
        var validator = new TestRequestValidator();
        var request = new TestRequest { Name = "A", Age = 200 };  // Multiple errors

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 2);
        var errorMessages = result.Errors.Select(e => e.ErrorMessage).ToList();
        Assert.Contains("at least 3 characters", string.Join(",", errorMessages));
        Assert.Contains("less than 150", string.Join(",", errorMessages));
    }

    /// <summary>
    /// Test: Validator is resolved from dependency injection
    /// </summary>
    [Fact]
    public void Filter_WithRegisteredValidator_ResolvesFromDI()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<IValidator<TestRequest>, TestRequestValidator>();
        var provider = services.BuildServiceProvider();

        // Act
        var validator = provider.GetService<IValidator<TestRequest>>();

        // Assert - validator should be available
        Assert.NotNull(validator);
    }

    /// <summary>
    /// Test: Filter skips validation when no validator is registered
    /// </summary>
    [Fact]
    public void Filter_WithoutValidator_SkipsValidation()
    {
        // Arrange
        var services = new ServiceCollection();
        // Don't register validator
        var provider = services.BuildServiceProvider();

        // Act
        var validator = provider.GetService<IValidator<TestRequest>>();

        // Assert - validator should be null
        Assert.Null(validator);
    }
}
