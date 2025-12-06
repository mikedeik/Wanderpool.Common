using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Wanderpool.Common.Clients.Exceptions;
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
        response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        envelope.Should().NotBeNull();
        envelope.IsSuccess.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error.Code.Should().Be("VALIDATION_ERROR");
        envelope.TraceId.Should().NotBeNullOrEmpty();
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
        response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        envelope.Should().NotBeNull();
        envelope.IsSuccess.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error.Code.Should().Be("UNAUTHORIZED");
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
        response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        envelope.Should().NotBeNull();
        envelope.IsSuccess.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error.Code.Should().Be("FORBIDDEN");
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
        response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        envelope.Should().NotBeNull();
        envelope.IsSuccess.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error.Code.Should().Be("NOT_FOUND");
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
        response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        envelope.Should().NotBeNull();
        envelope.IsSuccess.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error.Code.Should().Be("CONFLICT");
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
        response.StatusCode.Should().Be(StatusCodes.Status499ClientClosedRequest);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        envelope.Should().NotBeNull();
        envelope.IsSuccess.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error.Code.Should().Be("CLIENT_CLOSED");
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
        response.StatusCode.Should().Be(StatusCodes.Status502BadGateway);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        envelope.Should().NotBeNull();
        envelope.IsSuccess.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error.Code.Should().Be("UPSTREAM_ERROR");
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
        response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        envelope.Should().NotBeNull();
        envelope.IsSuccess.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error.Code.Should().Be("INTERNAL_ERROR");
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

        envelope.Should().NotBeNull();
        envelope.TraceId.Should().NotBeNullOrEmpty();
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
        response.Content.Headers.ContentType.Should().NotBeNull();
        response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
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
        response.StatusCode.Should().Be(StatusCodes.Status200OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("\"Success\"");
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
        envelope.Error.Level.Should().Be(ApiResponseErrorLevel.Error);
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
        envelope.Error.Level.Should().Be(ApiResponseErrorLevel.Warning);
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
        response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        var envelope = JsonSerializer.Deserialize<ApiResponseEnvelope<object>>(content);

        envelope.Should().NotBeNull();
        envelope.IsSuccess.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error.Code.Should().Be("BUSINESS_RULE_VIOLATION");
    }
}
