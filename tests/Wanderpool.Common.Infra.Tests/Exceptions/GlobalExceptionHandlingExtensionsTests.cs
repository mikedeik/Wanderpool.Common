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
        Assert.NotNull(result);
        Assert.Equal(app, result); // Should return the app for method chaining
    }

    [Fact]
    public async Task UseWanderpoolExceptionHandling_CatchesExceptionsAndReturnsConsistentResponse()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseWanderpoolExceptionHandling();
        app.MapGet("/error", () => { throw new InvalidOperationException("Test error"); });

        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/error");

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, (int)response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        Assert.NotNull(envelope);
        Assert.False(envelope.IsSuccess);
        Assert.NotNull(envelope.Error);
        Assert.Equal("INTERNAL_ERROR", envelope.Error.Code);
        Assert.NotNull(envelope.TraceId);
        Assert.True(!string.IsNullOrEmpty(envelope.TraceId));
    }

    [Fact]
    public async Task UseWanderpoolExceptionHandling_AllowsSuccessfulRequestsToPass()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseWanderpoolExceptionHandling();
        app.MapGet("/success", () => "Hello World");

        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/success");

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("\"Hello World\"", content);
    }

    [Fact]
    public async Task UseWanderpoolExceptionHandling_CanBeCalledMultipleTimes()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        // Act - calling it multiple times should not cause issues
        app.UseWanderpoolExceptionHandling();
        app.UseWanderpoolExceptionHandling();

        app.MapGet("/test", () => { throw new InvalidOperationException("Error"); });

        await app.StartAsync();
        var client = app.GetTestClient();

        // Assert
        var response = await client.GetAsync("/test");
        Assert.Equal(StatusCodes.Status500InternalServerError, (int)response.StatusCode);
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
        Assert.NotNull(result);
        Assert.Equal(app, result);
    }

    [Fact]
    public async Task UseWanderpoolExceptionHandling_WorksWithValidationException()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
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

        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/validate");

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        Assert.NotNull(envelope);
        Assert.Equal("VALIDATION_ERROR", envelope.Error.Code);
    }

    [Fact]
    public async Task UseWanderpoolExceptionHandling_WorksWithNotFoundException()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseWanderpoolExceptionHandling();
        app.MapGet("/notfound", () =>
        {
            throw new NotFoundException("User", "123");
        });

        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/notfound");

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        Assert.NotNull(envelope);
        Assert.Equal("NOT_FOUND", envelope.Error.Code);
    }

    [Fact]
    public async Task UseWanderpoolExceptionHandling_ReturnsJsonContentType()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseWanderpoolExceptionHandling();
        app.MapGet("/error", () => { throw new InvalidOperationException("Test"); });

        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/error");

        // Assert
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
    }

    [Fact]
    public async Task UseWanderpoolExceptionHandling_IncludesTraceIdInErrorResponse()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseWanderpoolExceptionHandling();
        app.MapGet("/error", () => { throw new Exception("Test error"); });

        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/error");
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        // Assert
        Assert.NotNull(envelope.TraceId);
        Assert.True(!string.IsNullOrEmpty(envelope.TraceId));
    }

    [Fact]
    public async Task UseWanderpoolExceptionHandling_SetsCorrectErrorLevel()
    {
        // Arrange - 5xx error
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseWanderpoolExceptionHandling();
        app.MapGet("/error", () => { throw new Exception("Internal error"); });

        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/error");
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        // Assert
        Assert.Equal(ApiResponseErrorLevel.Error, envelope.Error.Level);
    }
}
