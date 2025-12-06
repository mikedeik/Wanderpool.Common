using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
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

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

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
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new UnauthorizedAccessException("Access denied.");
        });

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

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
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new ForbiddenException("You do not have permission to access this resource.");
        });

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

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
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new NotFoundException("User", "123");
        });

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

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
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new ConflictException("This resource already exists with a different version.", "resource-123");
        });

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status409Conflict, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

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
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new OperationCanceledException("Operation was cancelled.");
        });

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status499ClientClosedRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

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
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new RemoteServiceException(500, "Remote service error");
        });

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status502BadGateway, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

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
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new InvalidOperationException("Something went wrong.");
        });

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

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
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new InvalidOperationException("Test error");
        });

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        Assert.NotNull(envelope);
        Assert.NotNull(envelope.TraceId);
        Assert.True(!string.IsNullOrEmpty(envelope.TraceId));
    }

    [Fact]
    public async Task Middleware_ReturnsJsonContentType()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new InvalidOperationException("Test error");
        });

        var server = new TestServer(app);
        var client = server.CreateClient();

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
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () => "Success");

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("\"Success\"", content);
    }

    [Fact]
    public async Task Middleware_ErrorLevel_IsErrorFor5xxStatusCodes()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new InvalidOperationException("Server error");
        });

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/test");
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        // Assert
        Assert.Equal(ApiResponseErrorLevel.Error, envelope.Error.Level);
    }

    [Fact]
    public async Task Middleware_ErrorLevel_IsWarningFor4xxStatusCodes()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new NotFoundException("Not found", "Resource", "123");
        });

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/test");
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        // Assert
        Assert.Equal(ApiResponseErrorLevel.Warning, envelope.Error.Level);
    }

    [Fact]
    public async Task Middleware_HandlesBuisinessRuleException_Returns500()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new BusinessRuleException("MinimumAge", "User must be at least 18 years old.");
        });

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        Assert.NotNull(envelope);
        Assert.False(envelope.IsSuccess);
        Assert.NotNull(envelope.Error);
        Assert.Equal("BUSINESS_RULE_VIOLATION", envelope.Error.Code);
    }

    [Fact]
    public async Task Middleware_InProductionMode_HidesDetailedErrorMessagesFor5xxErrors()
    {
        // Arrange
        var args = new[] { "--environment=Production" };
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new InvalidOperationException("Sensitive internal error details: Database connection string exposed!");
        });

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Error);
        Assert.Equal("An unexpected error occurred. Please try again later.", envelope.Error.Message);
        Assert.DoesNotContain("Database connection string", envelope.Error.Message);
    }

    [Fact]
    public async Task Middleware_InDevelopmentMode_ShowsDetailedErrorMessages()
    {
        // Arrange
        var args = new[] { "--environment=Development" };
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new InvalidOperationException("Detailed error message for debugging.");
        });

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

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
        var app = builder.Build();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.MapGet("/test", () =>
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Email", new[] { "Email is required" } }
            });
        });

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Error);
        // 4xx errors should show their actual messages even in production
        Assert.Contains("Validation failed", envelope.Error.Message);
    }
}
