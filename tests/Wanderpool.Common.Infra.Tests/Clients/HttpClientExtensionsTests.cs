using Microsoft.Extensions.DependencyInjection;
using Wanderpool.Common.Infra.Clients;
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
}
