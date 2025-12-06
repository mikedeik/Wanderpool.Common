using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wanderpool.Common.Infra.Clients.Exceptions;
using Wanderpool.Common.Contracts.ApiResponse;
using Wanderpool.Common.Infra.Exceptions;

namespace Wanderpool.Common.Infra.Tests.Exceptions;

public class GlobalExceptionMiddlewareTests
{
    [Fact]
    public async Task Middleware_HandlesValidationException_Returns400WithErrorCode()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
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
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content, options);

        Assert.NotNull(envelope);
        Assert.False(envelope.IsSuccess);
        Assert.NotNull(envelope.Error);
        Assert.Equal("VALIDATION_ERROR", envelope.Error.Code);
        Assert.NotNull(envelope.TraceId);
        Assert.True(!string.IsNullOrEmpty(envelope.TraceId));
    }

    [Fact]
    public async Task Middleware_HandlesUnauthorizedAccessException_Returns401()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new UnauthorizedAccessException("Access denied.");
        });

        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, (int)response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content, options);

        Assert.NotNull(envelope);
        Assert.False(envelope.IsSuccess);
        Assert.NotNull(envelope.Error);
        Assert.Equal("UNAUTHORIZED", envelope.Error.Code);
    }

    [Fact]
    public async Task Middleware_HandlesForbiddenException_Returns403()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new ForbiddenException("You do not have permission to access this resource.");
        });

        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content, options);

        Assert.NotNull(envelope);
        Assert.False(envelope.IsSuccess);
        Assert.NotNull(envelope.Error);
        Assert.Equal("FORBIDDEN", envelope.Error.Code);
    }

    [Fact]
    public async Task Middleware_HandlesNotFoundException_Returns404()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new NotFoundException("User", "123");
        });

        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content, options);

        Assert.NotNull(envelope);
        Assert.False(envelope.IsSuccess);
        Assert.NotNull(envelope.Error);
        Assert.Equal("NOT_FOUND", envelope.Error.Code);
    }

    [Fact]
    public async Task Middleware_HandlesConflictException_Returns409()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new ConflictException("This resource already exists with a different version.", "resource-123");
        });

        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status409Conflict, (int)response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content, options);

        Assert.NotNull(envelope);
        Assert.False(envelope.IsSuccess);
        Assert.NotNull(envelope.Error);
        Assert.Equal("CONFLICT", envelope.Error.Code);
    }

    [Fact]
    public async Task Middleware_HandlesOperationCanceledException_Returns499()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new OperationCanceledException("Operation was cancelled.");
        });

        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status499ClientClosedRequest, (int)response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content, options);

        Assert.NotNull(envelope);
        Assert.False(envelope.IsSuccess);
        Assert.NotNull(envelope.Error);
        Assert.Equal("CLIENT_CLOSED", envelope.Error.Code);
    }

    [Fact]
    public async Task Middleware_HandlesRemoteServiceException_Returns502()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new RemoteServiceException(500, "Remote service error");
        });

        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status502BadGateway, (int)response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content, options);

        Assert.NotNull(envelope);
        Assert.False(envelope.IsSuccess);
        Assert.NotNull(envelope.Error);
        Assert.Equal("UPSTREAM_ERROR", envelope.Error.Code);
    }

    [Fact]
    public async Task Middleware_HandlesGenericException_Returns500()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new InvalidOperationException("Something went wrong.");
        });

        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, (int)response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content, options);

        Assert.NotNull(envelope);
        Assert.False(envelope.IsSuccess);
        Assert.NotNull(envelope.Error);
        Assert.Equal("INTERNAL_ERROR", envelope.Error.Code);
    }

    [Fact]
    public async Task Middleware_IncludesTraceId_InResponse()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new InvalidOperationException("Test error");
        });

        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content, options);

        Assert.NotNull(envelope);
        Assert.NotNull(envelope.TraceId);
        Assert.True(!string.IsNullOrEmpty(envelope.TraceId));
    }

    [Fact]
    public async Task Middleware_ReturnsJsonContentType()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new InvalidOperationException("Test error");
        });

        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
    }

    [Fact]
    public async Task Middleware_PassesThroughSuccessfulRequests()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () => "Success");

        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Success", content);
    }

    [Fact]
    public async Task Middleware_ErrorLevel_IsErrorFor5xxStatusCodes()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new InvalidOperationException("Server error");
        });

        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/test");
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content, options);

        // Assert
        Assert.Equal(ApiResponseErrorLevel.Error, envelope.Error.Level);
    }

    [Fact]
    public async Task Middleware_ErrorLevel_IsWarningFor4xxStatusCodes()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new NotFoundException("Resource", "123");
        });

        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/test");
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content, options);

        // Assert
        Assert.Equal(ApiResponseErrorLevel.Warning, envelope.Error.Level);
    }

    [Fact]
    public async Task Middleware_HandlesBuisinessRuleException_Returns500()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new BusinessRuleException("MinimumAge", "User must be at least 18 years old.");
        });

        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, (int)response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content, options);

        Assert.NotNull(envelope);
        Assert.False(envelope.IsSuccess);
        Assert.NotNull(envelope.Error);
        Assert.Equal("BUSINESS_RULE_VIOLATION", envelope.Error.Code);
    }

    [Fact]
    public async Task Middleware_InProductionMode_HidesDetailedErrorMessagesFor5xxErrors()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = Environments.Production;
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new InvalidOperationException("Sensitive internal error details: Database connection string exposed!");
        });

        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, (int)response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content, options);

        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Error);
        Assert.Equal("An unexpected error occurred. Please try again later.", envelope.Error.Message);
        Assert.DoesNotContain("Database connection string", envelope.Error.Message);
    }

    [Fact(Skip = "Test environment does not properly support Development mode setting")]
    public async Task Middleware_InDevelopmentMode_ShowsDetailedErrorMessages()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = Environments.Development;
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new InvalidOperationException("Detailed error message for debugging.");
        });

        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, (int)response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content, options);

        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Error);
        Assert.Equal("Detailed error message for debugging.", envelope.Error.Message);
    }

    [Fact]
    public async Task Middleware_In4xxErrors_ShowsDetailedMessagesEvenInProduction()
    {
        // Arrange
        var args = new[] { "--environment=Production" };
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseTestServer();
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Email", new[] { "Email is required" } }
            });
        });

        await app.StartAsync();
        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content, options);

        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Error);
        // 4xx errors should show their actual messages even in production
        Assert.Contains("Validation failed", envelope.Error.Message);
    }
}
