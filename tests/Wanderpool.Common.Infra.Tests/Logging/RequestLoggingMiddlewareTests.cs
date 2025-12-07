using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Wanderpool.Common.Infra.Logging;
using Wanderpool.Common.Infra.Telemetry;

namespace Wanderpool.Common.Infra.Tests.Logging;

/// <summary>
/// Tests for RequestLoggingMiddleware.
/// </summary>
public class RequestLoggingMiddlewareTests
{
    [Fact]
    public async Task Middleware_AllowsRequestsToPassThrough()
    {
        // Arrange
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddWanderpoolCorrelationId();
                services.Configure<RequestLoggingOptions>(options =>
                {
                    options.EnableRequestBodyLogging = false;
                    options.EnableResponseBodyLogging = false;
                });
            })
            .Configure(app =>
            {
                app.UseWanderpoolCorrelationId();
                app.UseWanderpoolRequestLogging();
                app.Run(async context => await context.Response.WriteAsync("Success"));
            });

        using var testServer = new TestServer(builder);
        using var httpClient = testServer.CreateClient();

        // Act
        var response = await httpClient.GetAsync("/test");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Middleware_PreservesResponseBody()
    {
        // Arrange
        var expectedBody = "Test Response Body";
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddWanderpoolCorrelationId();
                services.Configure<RequestLoggingOptions>(options =>
                {
                    options.EnableRequestBodyLogging = false;
                    options.EnableResponseBodyLogging = false;
                });
            })
            .Configure(app =>
            {
                app.UseWanderpoolCorrelationId();
                app.UseWanderpoolRequestLogging();
                app.Run(async context => await context.Response.WriteAsync(expectedBody));
            });

        using var testServer = new TestServer(builder);
        using var httpClient = testServer.CreateClient();

        // Act
        var response = await httpClient.GetAsync("/test");
        var actualBody = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(expectedBody, actualBody);
    }

    [Fact]
    public async Task Middleware_IncludesCorrelationIdInContext()
    {
        // Arrange
        var correlationId = "test-correlation-id-123";
        string? capturedCorrelationId = null;

        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddWanderpoolCorrelationId();
                services.Configure<RequestLoggingOptions>(options =>
                {
                    options.EnableRequestBodyLogging = false;
                    options.EnableResponseBodyLogging = false;
                });
            })
            .Configure(app =>
            {
                app.UseWanderpoolCorrelationId();
                app.UseWanderpoolRequestLogging();
                app.Run(context =>
                {
                    capturedCorrelationId = context.Items["CorrelationId"]?.ToString();
                    return context.Response.WriteAsync("OK");
                });
            });

        using var testServer = new TestServer(builder);
        using var httpClient = testServer.CreateClient();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, "/test");
        request.Headers.Add("X-Correlation-Id", correlationId);
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(correlationId, capturedCorrelationId);
    }

    [Fact]
    public async Task Middleware_DoesNotLogRequestBodyByDefault()
    {
        // Arrange
        var sensitiveData = "SENSITIVE_REQUEST_BODY_DATA";
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddWanderpoolCorrelationId();
                services.Configure<RequestLoggingOptions>(options =>
                {
                    options.EnableRequestBodyLogging = false;
                    options.EnableResponseBodyLogging = false;
                });
            })
            .Configure(app =>
            {
                app.UseWanderpoolCorrelationId();
                app.UseWanderpoolRequestLogging();
                app.Run(async context =>
                {
                    context.Response.StatusCode = 201;
                    await context.Response.WriteAsync("Created");
                });
            });

        using var testServer = new TestServer(builder);
        using var httpClient = testServer.CreateClient();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Post, "/test");
        request.Content = new StringContent(sensitiveData);
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Middleware_PreservesResponseContentType()
    {
        // Arrange
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddWanderpoolCorrelationId();
                services.Configure<RequestLoggingOptions>(options =>
                {
                    options.EnableRequestBodyLogging = false;
                    options.EnableResponseBodyLogging = false;
                });
            })
            .Configure(app =>
            {
                app.UseWanderpoolCorrelationId();
                app.UseWanderpoolRequestLogging();
                app.Run(async context =>
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"status\":\"ok\"}");
                });
            });

        using var testServer = new TestServer(builder);
        using var httpClient = testServer.CreateClient();

        // Act
        var response = await httpClient.GetAsync("/test");

        // Assert
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Middleware_Returns404ForNonExistentEndpoint()
    {
        // Arrange
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddWanderpoolCorrelationId();
                services.Configure<RequestLoggingOptions>(options =>
                {
                    options.EnableRequestBodyLogging = false;
                    options.EnableResponseBodyLogging = false;
                });
            })
            .Configure(app =>
            {
                app.UseWanderpoolCorrelationId();
                app.UseWanderpoolRequestLogging();
                app.Run(async context =>
                {
                    // This endpoint intentionally doesn't exist
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Not Found");
                });
            });

        using var testServer = new TestServer(builder);
        using var httpClient = testServer.CreateClient();

        // Act
        var response = await httpClient.GetAsync("/nonexistent");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Middleware_PreservesHttpMethod()
    {
        // Arrange
        string? capturedMethod = null;
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddWanderpoolCorrelationId();
                services.Configure<RequestLoggingOptions>(options =>
                {
                    options.EnableRequestBodyLogging = false;
                    options.EnableResponseBodyLogging = false;
                });
            })
            .Configure(app =>
            {
                app.UseWanderpoolCorrelationId();
                app.UseWanderpoolRequestLogging();
                app.Run(context =>
                {
                    capturedMethod = context.Request.Method;
                    return context.Response.WriteAsync("OK");
                });
            });

        using var testServer = new TestServer(builder);
        using var httpClient = testServer.CreateClient();

        // Act
        var response = await httpClient.GetAsync("/test");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("GET", capturedMethod);
    }
}
