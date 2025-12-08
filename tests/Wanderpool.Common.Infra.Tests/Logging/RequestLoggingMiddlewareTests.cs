using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wanderpool.Common.Infra.Logging;
using Wanderpool.Common.Infra.Telemetry;

namespace Wanderpool.Common.Infra.Tests.Logging;

/// <summary>
/// Tests for RequestLoggingMiddleware.
/// </summary>
public class RequestLoggingMiddlewareTests
{
    /// <summary>
    /// Creates a TestServer with standard middleware configuration.
    /// </summary>
    private static TestServer CreateTestServer(
        Action<RequestLoggingOptions>? configureOptions = null,
        RequestDelegate? requestHandler = null)
    {
        var host = new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddWanderpoolCorrelationId();
                    services.Configure<RequestLoggingOptions>(options =>
                    {
                        options.EnableRequestBodyLogging = false;
                        options.EnableResponseBodyLogging = false;
                        configureOptions?.Invoke(options);
                    });
                    services.AddRouting();
                });
                webBuilder.Configure(app =>
                {
                    app.UseWanderpoolCorrelationId();
                    app.UseWanderpoolRequestLogging();
                    if (requestHandler != null)
                    {
                        app.Run(requestHandler);
                    }
                    else
                    {
                        app.Run(async context => await context.Response.WriteAsync("Success"));
                    }
                });
            })
            .Build();

        host.Start();
        return host.GetTestServer();
    }

    [Fact]
    public async Task Middleware_AllowsRequestsToPassThrough()
    {
        // Arrange
        using var testServer = CreateTestServer();
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
        using var testServer = CreateTestServer(
            requestHandler: async context => await context.Response.WriteAsync(expectedBody));
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

        using var testServer = CreateTestServer(
            requestHandler: context =>
            {
                capturedCorrelationId = context.Items["CorrelationId"]?.ToString();
                return context.Response.WriteAsync("OK");
            });
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
        using var testServer = CreateTestServer(
            requestHandler: async context =>
            {
                context.Response.StatusCode = 201;
                await context.Response.WriteAsync("Created");
            });
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
        using var testServer = CreateTestServer(
            requestHandler: async context =>
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"status\":\"ok\"}");
            });
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
        using var testServer = CreateTestServer(
            requestHandler: async context =>
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Not Found");
            });
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
        using var testServer = CreateTestServer(
            requestHandler: context =>
            {
                capturedMethod = context.Request.Method;
                return context.Response.WriteAsync("OK");
            });
        using var httpClient = testServer.CreateClient();

        // Act
        var response = await httpClient.GetAsync("/test");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("GET", capturedMethod);
    }

    [Fact]
    public async Task Middleware_CanRedactAuthorizationHeader()
    {
        // Arrange
        var authorizationValue = "Bearer secret-token-12345";
        using var testServer = CreateTestServer(
            requestHandler: async context => await context.Response.WriteAsync("OK"));
        using var httpClient = testServer.CreateClient();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, "/test");
        request.Headers.Add("Authorization", authorizationValue);
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Middleware_CanRedactXApiKeyHeader()
    {
        // Arrange
        var apiKeyValue = "super-secret-api-key";
        using var testServer = CreateTestServer(
            requestHandler: async context => await context.Response.WriteAsync("OK"));
        using var httpClient = testServer.CreateClient();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, "/test");
        request.Headers.Add("X-Api-Key", apiKeyValue);
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Middleware_CanRedactCookieHeader()
    {
        // Arrange
        var cookieValue = "session=abc123def456";
        using var testServer = CreateTestServer(
            requestHandler: async context => await context.Response.WriteAsync("OK"));
        using var httpClient = testServer.CreateClient();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, "/test");
        request.Headers.Add("Cookie", cookieValue);
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Middleware_PreservesNonSensitiveHeaders()
    {
        // Arrange
        var customHeaderValue = "custom-value";
        string? capturedCustomHeader = null;

        using var testServer = CreateTestServer(
            requestHandler: context =>
            {
                capturedCustomHeader = context.Request.Headers["X-Custom-Header"].ToString();
                return context.Response.WriteAsync("OK");
            });
        using var httpClient = testServer.CreateClient();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, "/test");
        request.Headers.Add("X-Custom-Header", customHeaderValue);
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(customHeaderValue, capturedCustomHeader);
    }

    [Fact]
    public async Task Middleware_AllowsConfigurableRedactionList()
    {
        // Arrange
        using var testServer = CreateTestServer(
            configureOptions: options => options.SensitiveHeaders.Add("X-Custom-Secret"),
            requestHandler: async context => await context.Response.WriteAsync("OK"));
        using var httpClient = testServer.CreateClient();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, "/test");
        request.Headers.Add("X-Custom-Secret", "secret-value");
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Middleware_LogsRequestBodyWhenEnabled()
    {
        // Arrange
        var requestBody = "request-body-data";
        using var testServer = CreateTestServer(
            configureOptions: options => options.EnableRequestBodyLogging = true,
            requestHandler: async context => await context.Response.WriteAsync("OK"));
        using var httpClient = testServer.CreateClient();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Post, "/test");
        request.Content = new StringContent(requestBody);
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Middleware_DoesNotLogRequestBodyWhenDisabled()
    {
        // Arrange
        var requestBody = "secret-request-data";
        using var testServer = CreateTestServer(
            requestHandler: async context => await context.Response.WriteAsync("OK"));
        using var httpClient = testServer.CreateClient();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Post, "/test");
        request.Content = new StringContent(requestBody);
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Middleware_LogsResponseBodyWhenEnabled()
    {
        // Arrange
        var responseBody = "response-body-data";
        using var testServer = CreateTestServer(
            configureOptions: options => options.EnableResponseBodyLogging = true,
            requestHandler: async context => await context.Response.WriteAsync(responseBody));
        using var httpClient = testServer.CreateClient();

        // Act
        var response = await httpClient.GetAsync("/test");
        var actualBody = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(responseBody, actualBody);
    }

    [Fact]
    public async Task Middleware_ProperlyHandlesRequestBodyStreams()
    {
        // Arrange
        var requestBody = "test-body-content";
        string? capturedRequestBody = null;

        using var testServer = CreateTestServer(
            configureOptions: options => options.EnableRequestBodyLogging = true,
            requestHandler: async context =>
            {
                using var reader = new StreamReader(context.Request.Body);
                capturedRequestBody = await reader.ReadToEndAsync();
                await context.Response.WriteAsync("OK");
            });
        using var httpClient = testServer.CreateClient();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Post, "/test");
        request.Content = new StringContent(requestBody);
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(requestBody, capturedRequestBody);
    }

    [Fact]
    public async Task Middleware_TruncatesLargeBodies()
    {
        // Arrange
        var largeRequestBody = new string('x', 10000);
        var expectedResponseBody = new string('y', 8000);
        using var testServer = CreateTestServer(
            configureOptions: options =>
            {
                options.EnableRequestBodyLogging = true;
                options.EnableResponseBodyLogging = true;
                options.MaxBodySizeLogged = 1024;
            },
            requestHandler: async context => await context.Response.WriteAsync(expectedResponseBody));
        using var httpClient = testServer.CreateClient();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Post, "/test");
        request.Content = new StringContent(largeRequestBody);
        var response = await httpClient.SendAsync(request);
        var actualBody = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedResponseBody, actualBody);
    }
}
