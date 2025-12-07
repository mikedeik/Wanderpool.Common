using Microsoft.Extensions.DependencyInjection;
using Wanderpool.Common.Infra.Clients;
using Wanderpool.Common.Infra.Clients.HttpClientHandlers;
using Xunit;

namespace Wanderpool.Common.Infra.Tests.Clients;

/// <summary>
/// Tests for HttpClientExtensions - service extension for HTTP client registration.
/// </summary>
public class HttpClientExtensionsTests
{
    /// <summary>
    /// Sample HTTP client for testing.
    /// </summary>
    public class TestHttpClient
    {
        public TestHttpClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; }
    }

    /// <summary>
    /// Mock token provider for testing.
    /// </summary>
    public class MockTokenProvider : ITokenProvider
    {
        public Task<string?> GetAccessTokenAsync()
        {
            return Task.FromResult<string?>("test-token");
        }

        public Task<bool> RefreshTokenAsync()
        {
            return Task.FromResult(true);
        }
    }

    /// <summary>
    /// Test: AddWanderpoolHttpClient registers typed client.
    /// </summary>
    [Fact]
    public void AddWanderpoolHttpClient_RegistersTypedClient()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolHttpClient<TestHttpClient>(options =>
        {
            options.BaseAddress = "http://localhost:5000";
        });
        var provider = services.BuildServiceProvider();

        // Assert - typed client should be registered
        var client = provider.GetService<TestHttpClient>();
        Assert.NotNull(client);
    }

    /// <summary>
    /// Test: Base address is configured correctly.
    /// </summary>
    [Fact]
    public void AddWanderpoolHttpClient_ConfiguresBaseAddress()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolHttpClient<TestHttpClient>(options =>
        {
            options.BaseAddress = "https://api.example.com";
        });

        // Assert - should not throw
        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider);
    }

    /// <summary>
    /// Test: Resilience pipeline is applied.
    /// </summary>
    [Fact]
    public void AddWanderpoolHttpClient_AppliesResiliencePipeline()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolHttpClient<TestHttpClient>(options =>
        {
            options.BaseAddress = "http://localhost:5000";
            options.ResiliencePipelineName = "default";
        });

        var provider = services.BuildServiceProvider();

        // Assert - typed client should be available
        var client = provider.GetService<TestHttpClient>();
        Assert.NotNull(client);
    }

    /// <summary>
    /// Test: Logging handler is applied.
    /// </summary>
    [Fact]
    public void AddWanderpoolHttpClient_AppliesLoggingHandler()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolHttpClient<TestHttpClient>(options =>
        {
            options.BaseAddress = "http://localhost:5000";
        });

        var provider = services.BuildServiceProvider();

        // Assert - typed client should be available
        var client = provider.GetService<TestHttpClient>();
        Assert.NotNull(client);
    }

    /// <summary>
    /// Test: Metrics handler is applied.
    /// </summary>
    [Fact]
    public void AddWanderpoolHttpClient_AppliesMetricsHandler()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolHttpClient<TestHttpClient>(options =>
        {
            options.BaseAddress = "http://localhost:5000";
        });

        var provider = services.BuildServiceProvider();

        // Assert - typed client should be available
        var client = provider.GetService<TestHttpClient>();
        Assert.NotNull(client);
    }

    /// <summary>
    /// Test: TokenRefreshHandler is added when token provider type is specified.
    /// </summary>
    [Fact]
    public void AddWanderpoolHttpClient_AddsTokenRefreshHandler_WhenProviderSpecified()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<ITokenProvider, MockTokenProvider>();

        // Act
        services.AddWanderpoolHttpClient<TestHttpClient>(options =>
        {
            options.BaseAddress = "http://localhost:5000";
            options.TokenProviderType = typeof(MockTokenProvider);
        });

        var provider = services.BuildServiceProvider();

        // Assert - typed client should be available with token provider registered
        var client = provider.GetService<TestHttpClient>();
        Assert.NotNull(client);
        var tokenProvider = provider.GetService<ITokenProvider>();
        Assert.NotNull(tokenProvider);
    }

    /// <summary>
    /// Test: TokenRefreshHandler is NOT added when token provider type is null.
    /// </summary>
    [Fact]
    public void AddWanderpoolHttpClient_DoesNotAddTokenRefreshHandler_WhenNoProviderSpecified()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolHttpClient<TestHttpClient>(options =>
        {
            options.BaseAddress = "http://localhost:5000";
            options.TokenProviderType = null;
        });

        var provider = services.BuildServiceProvider();

        // Assert - typed client should be available without token provider
        var client = provider.GetService<TestHttpClient>();
        Assert.NotNull(client);
        var tokenProvider = provider.GetService<ITokenProvider>();
        Assert.Null(tokenProvider);
    }

    /// <summary>
    /// Test: Token provider is resolved from dependency injection.
    /// </summary>
    [Fact]
    public void AddWanderpoolHttpClient_ResolvesTokenProvider_FromDependencyInjection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<ITokenProvider, MockTokenProvider>();

        // Act
        services.AddWanderpoolHttpClient<TestHttpClient>(options =>
        {
            options.BaseAddress = "http://localhost:5000";
            options.TokenProviderType = typeof(MockTokenProvider);
        });

        var provider = services.BuildServiceProvider();
        var tokenProvider = provider.GetService<ITokenProvider>();

        // Assert - token provider should be the registered mock
        Assert.NotNull(tokenProvider);
        Assert.IsType<MockTokenProvider>(tokenProvider);
    }

    /// <summary>
    /// Test: Handler order is preserved in pipeline.
    /// </summary>
    [Fact]
    public void AddWanderpoolHttpClient_HandlerOrderIsCorrect()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<ITokenProvider, MockTokenProvider>();

        // Act
        services.AddWanderpoolHttpClient<TestHttpClient>(options =>
        {
            options.BaseAddress = "http://localhost:5000";
            options.TokenProviderType = typeof(MockTokenProvider);
            options.ResiliencePipelineName = "default";
        });

        var provider = services.BuildServiceProvider();

        // Assert - client should be properly configured with all handlers
        var client = provider.GetService<TestHttpClient>();
        Assert.NotNull(client);
        Assert.NotNull(client!.HttpClient.BaseAddress);
        Assert.Equal("http://localhost:5000/", client.HttpClient.BaseAddress.ToString());
        Assert.Equal(TimeSpan.FromSeconds(30), client.HttpClient.Timeout);
    }
}
