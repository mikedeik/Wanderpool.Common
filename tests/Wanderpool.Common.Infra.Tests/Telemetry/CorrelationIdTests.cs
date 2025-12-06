using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Wanderpool.Common.Infra.Telemetry;

namespace Wanderpool.Common.Infra.Tests.Telemetry;

public class CorrelationIdTests
{
    [Fact]
    public void CorrelationContext_StoresCorrelationId()
    {
        // Arrange
        var correlationId = Guid.NewGuid().ToString("D");

        // Act
        var context = new CorrelationContext(correlationId);

        // Assert
        Assert.Equal(correlationId, context.CorrelationId);
    }

    [Fact]
    public void CorrelationContext_ThrowsOnNullCorrelationId()
    {
        // Act & Assert
        var action = () => new CorrelationContext(null!);
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void CorrelationContext_ThrowsOnEmptyCorrelationId()
    {
        // Act & Assert
        var action = () => new CorrelationContext(string.Empty);
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public async Task CorrelationIdMiddleware_GeneratesCorrelationId_WhenNotProvided()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddHttpContextAccessor();
        var app = builder.Build();

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.MapGet("/test", (HttpContext context) =>
        {
            var correlationId = context.Items["CorrelationId"];
            return Results.Ok(correlationId);
        });

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/test");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        Assert.NotNull(content);
        Assert.True(!string.IsNullOrEmpty(content));
        // Content should be a GUID string
        Assert.True(Guid.TryParse(content.Trim('"'), out _));
    }

    [Fact]
    public async Task CorrelationIdMiddleware_ExtractsCorrelationId_FromHeader()
    {
        // Arrange
        var correlationId = "test-correlation-123";
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddHttpContextAccessor();
        var app = builder.Build();

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.MapGet("/test", (HttpContext context) =>
        {
            var id = context.Items["CorrelationId"];
            return Results.Ok(id);
        });

        var server = new TestServer(app);
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Add("X-Correlation-Id", correlationId);

        // Act
        var response = await client.GetAsync("/test");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        Assert.Contains(correlationId, content);
    }

    [Fact]
    public async Task CorrelationIdMiddleware_IncludesCorrelationId_InResponseHeaders()
    {
        // Arrange
        var correlationId = "test-correlation-456";
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddHttpContextAccessor();
        var app = builder.Build();

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.MapGet("/test", () => Results.Ok());

        var server = new TestServer(app);
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Add("X-Correlation-Id", correlationId);

        // Act
        var response = await client.GetAsync("/test");

        // Assert
        Assert.True(response.Headers.Contains("X-Correlation-Id"));
        Assert.Equal(correlationId, response.Headers.GetValues("X-Correlation-Id").First());
    }

    [Fact]
    public void AddWanderpoolCorrelationId_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolCorrelationId();

        // Assert
        Assert.Contains(services, sd => sd.ServiceType == typeof(IHttpContextAccessor));
        Assert.Contains(services, sd => sd.ServiceType == typeof(ICorrelationContext));
    }

    [Fact]
    public void UseWanderpoolCorrelationId_ReturnsAppForChaining()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        // Act
        var result = app.UseWanderpoolCorrelationId();

        // Assert
        Assert.Equal(app, result);
    }

    [Fact]
    public async Task ICorrelationContext_RetrievesCorrelationIdFromHttpContext()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddWanderpoolCorrelationId();
        var app = builder.Build();

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.MapGet("/test", (ICorrelationContext correlationContext) =>
        {
            return Results.Ok(new { correlationContext.CorrelationId });
        });

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/test");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        Assert.NotNull(content);
        Assert.True(!string.IsNullOrEmpty(content));
        Assert.Contains("correlationId", content);
    }

    [Fact]
    public async Task CorrelationIdMiddleware_PassesThroughSuccessfulRequests()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddHttpContextAccessor();
        var app = builder.Build();

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.MapGet("/test", () => Results.Ok("success"));

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/test");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
        Assert.Equal("\"success\"", content);
    }

    [Fact]
    public async Task CorrelationIdMiddleware_GeneratesUniqueIds_ForDifferentRequests()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddHttpContextAccessor();
        var app = builder.Build();

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.MapGet("/test", (HttpContext context) =>
        {
            var correlationId = context.Items["CorrelationId"];
            return Results.Ok(correlationId);
        });

        var server = new TestServer(app);
        var client = server.CreateClient();

        // Act
        var response1 = await client.GetAsync("/test");
        var content1 = await response1.Content.ReadAsStringAsync();

        var response2 = await client.GetAsync("/test");
        var content2 = await response2.Content.ReadAsStringAsync();

        // Assert
        Assert.NotEqual(content2, content1);
    }

    [Fact]
    public async Task CorrelationIdMiddleware_PrefersProvidedId_OverGeneratedId()
    {
        // Arrange
        var providedId = "provided-id-789";
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddHttpContextAccessor();
        var app = builder.Build();

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.MapGet("/test", (HttpContext context) =>
        {
            var correlationId = context.Items["CorrelationId"];
            return Results.Ok(correlationId);
        });

        var server = new TestServer(app);
        var client = server.CreateClient();
        client.DefaultRequestHeaders.Add("X-Correlation-Id", providedId);

        // Act
        var response = await client.GetAsync("/test");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains(providedId, content);
    }
}
