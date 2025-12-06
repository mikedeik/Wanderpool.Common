using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Wanderpool.Common.Contracts.ApiResponse;
using Wanderpool.Common.Infra.Exceptions;

namespace Wanderpool.Common.Infra.Tests.Exceptions;

public class GlobalExceptionHandlingExtensionsTests
{
    [Fact]
    public async Task UseWanderpoolExceptionHandling_RegistersMiddleware()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        // Act
        var result = app.UseWanderpoolExceptionHandling();

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(app); // Should return the app for method chaining
    }

    [Fact]
    public async Task UseWanderpoolExceptionHandling_CatchesExceptionsAndReturnsConsistentResponse()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.UseWanderpoolExceptionHandling();
        app.MapGet("/error", () => throw new InvalidOperationException("Test error"));

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/error");

        // Assert
        response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        envelope.Should().NotBeNull();
        envelope.IsSuccess.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error.Code.Should().Be("INTERNAL_ERROR");
        envelope.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UseWanderpoolExceptionHandling_AllowsSuccessfulRequestsToPass()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.UseWanderpoolExceptionHandling();
        app.MapGet("/success", () => "Hello World");

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/success");

        // Assert
        response.StatusCode.Should().Be(StatusCodes.Status200OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("\"Hello World\"");
    }

    [Fact]
    public async Task UseWanderpoolExceptionHandling_CanBeCalledMultipleTimes()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        // Act - calling it multiple times should not cause issues
        app.UseWanderpoolExceptionHandling();
        app.UseWanderpoolExceptionHandling();

        app.MapGet("/test", () => throw new InvalidOperationException("Error"));

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Assert
        var response = await client.GetAsync("/test");
        response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task UseWanderpoolExceptionHandling_ReturnsTheApplicationForMethodChaining()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        // Act - should return the app to enable method chaining
        var result = app
            .UseWanderpoolExceptionHandling()
            .UseRouting();

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(app);
    }

    [Fact]
    public async Task UseWanderpoolExceptionHandling_WorksWithValidationException()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.UseWanderpoolExceptionHandling();
        app.MapGet("/validate", () =>
        {
            var errors = new Dictionary<string, string[]>
            {
                { "Email", new[] { "Email is required" } }
            };
            throw new ValidationException(errors);
        });

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/validate");

        // Assert
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        envelope.Should().NotBeNull();
        envelope.Error.Code.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task UseWanderpoolExceptionHandling_WorksWithNotFoundException()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.UseWanderpoolExceptionHandling();
        app.MapGet("/notfound", () =>
        {
            throw new NotFoundException("User", "123");
        });

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/notfound");

        // Assert
        response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        envelope.Should().NotBeNull();
        envelope.Error.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task UseWanderpoolExceptionHandling_ReturnsJsonContentType()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.UseWanderpoolExceptionHandling();
        app.MapGet("/error", () => throw new InvalidOperationException("Test"));

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/error");

        // Assert
        response.Content.Headers.ContentType.Should().NotBeNull();
        response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
    }

    [Fact]
    public async Task UseWanderpoolExceptionHandling_IncludesTraceIdInErrorResponse()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.UseWanderpoolExceptionHandling();
        app.MapGet("/error", () => throw new Exception("Test error"));

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/error");
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        // Assert
        envelope.TraceId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UseWanderpoolExceptionHandling_SetsCorrectErrorLevel()
    {
        // Arrange - 5xx error
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.UseWanderpoolExceptionHandling();
        app.MapGet("/error", () => throw new Exception("Internal error"));

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/error");
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        // Assert
        envelope.Error.Level.Should().Be(ApiResponseErrorLevel.Error);
    }
}
