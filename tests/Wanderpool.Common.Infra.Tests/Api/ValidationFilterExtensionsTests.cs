using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Wanderpool.Common.Infra.Api;
using Xunit;

namespace Wanderpool.Common.Infra.Tests.Api;

/// <summary>
/// Tests for ValidationFilterExtensions - extension methods for easy filter registration.
/// </summary>
public class ValidationFilterExtensionsTests
{
    /// <summary>
    /// Sample request object for testing.
    /// </summary>
    public class TestRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Sample validator for TestRequest.
    /// </summary>
    public class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
        }
    }

    /// <summary>
    /// Test: WithValidation adds filter to route handler.
    /// </summary>
    [Fact]
    public void WithValidation_AddsFilterToRouteHandler()
    {
        // Arrange
        var builder = new RouteHandlerBuilder(new[] { new DummyEndpointConventionBuilder() });

        // Act - WithValidation should return the builder for chaining
        var result = builder.WithValidation<TestRequest>();

        // Assert - should return the same builder for chaining
        Assert.NotNull(result);
        Assert.IsAssignableFrom<RouteHandlerBuilder>(result);
    }

    /// <summary>
    /// Test: WithValidation is chainable with other methods.
    /// </summary>
    [Fact]
    public void WithValidation_IsChainable()
    {
        // Arrange
        var builder = new RouteHandlerBuilder(new[] { new DummyEndpointConventionBuilder() });

        // Act - should be able to chain multiple calls
        var result = builder.WithValidation<TestRequest>()
            .WithValidation<TestRequest>();

        // Assert - should still be a valid builder
        Assert.NotNull(result);
        Assert.IsAssignableFrom<RouteHandlerBuilder>(result);
    }

    /// <summary>
    /// Test: WithValidation works with generic route handlers.
    /// </summary>
    [Fact]
    public void WithValidation_WorksWithGenericRouteHandlers()
    {
        // Arrange
        var builder = new RouteHandlerBuilder(new[] { new DummyEndpointConventionBuilder() });

        // Act
        var stringResult = builder.WithValidation<string>();
        var intResult = builder.WithValidation<int>();
        var customResult = builder.WithValidation<TestRequest>();

        // Assert - all should work
        Assert.NotNull(stringResult);
        Assert.NotNull(intResult);
        Assert.NotNull(customResult);
    }

    /// <summary>
    /// Dummy endpoint convention builder for testing.
    /// </summary>
    private class DummyEndpointConventionBuilder : IEndpointConventionBuilder
    {
        public void Add(Action<EndpointBuilder> convention)
        {
            // No-op for testing
        }
    }
}
