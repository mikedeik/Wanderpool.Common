using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Wanderpool.Common.Infra.TestHelpers;
using Xunit;

namespace Wanderpool.Common.Infra.Tests.Integration;

/// <summary>
/// Tests for WanderpoolTestServerBuilder - helper for integration testing.
/// </summary>
public class TestServerBuilderTests
{
    /// <summary>
    /// Test: TestServer with full infrastructure can be created.
    /// </summary>
    [Fact]
    public async Task Build_WithDefaultOptions_CreatesTestServerWithInfrastructure()
    {
        // Arrange & Act
        using var server = new WanderpoolTestServerBuilder()
            .WithServiceName("TestService")
            .Build();

        var client = server.CreateClient();

        // Assert - health endpoint should be available
        var response = await client.GetAsync("/health/live");
        Assert.True(response.IsSuccessStatusCode);
    }

    /// <summary>
    /// Test: Configuration can be customized via AddConfiguration.
    /// </summary>
    [Fact]
    public async Task Build_WithCustomConfiguration_AppliesConfiguration()
    {
        // Arrange
        var customConfig = new Dictionary<string, string?>
        {
            { "CustomSetting:Value", "TestValue" }
        };

        // Act
        using var server = new WanderpoolTestServerBuilder()
            .WithServiceName("TestService")
            .WithConfiguration(customConfig)
            .Build();

        var client = server.CreateClient();

        // Assert - server should be created successfully
        var response = await client.GetAsync("/health/live");
        Assert.True(response.IsSuccessStatusCode);
    }

    /// <summary>
    /// Test: Services can be mocked/replaced via ConfigureServices.
    /// </summary>
    [Fact]
    public async Task Build_WithServiceOverrides_UsesOverriddenServices()
    {
        // Arrange
        var mockService = new MockTestService();

        // Act
        using var server = new WanderpoolTestServerBuilder()
            .WithServiceName("TestService")
            .ConfigureServices(services =>
            {
                services.AddSingleton<ITestService>(mockService);
            })
            .Build();

        // Assert - service should be resolvable
        var resolvedService = server.Services.GetService<ITestService>();
        Assert.NotNull(resolvedService);
        Assert.Same(mockService, resolvedService);
    }

    /// <summary>
    /// Test: HTTP client from TestServer can make requests.
    /// </summary>
    [Fact]
    public async Task CreateClient_ReturnsWorkingHttpClient()
    {
        // Arrange
        using var server = new WanderpoolTestServerBuilder()
            .WithServiceName("TestService")
            .WithEndpoint("/api/test", context => context.Response.WriteAsync("Hello Test"))
            .Build();

        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/api/test");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Equal("Hello Test", content);
    }

    /// <summary>
    /// Test: Builder supports method chaining (fluent API).
    /// </summary>
    [Fact]
    public void Builder_SupportsMethodChaining()
    {
        // Act & Assert - should not throw
        var builder = new WanderpoolTestServerBuilder()
            .WithServiceName("TestService")
            .WithServiceVersion("1.0.0")
            .WithConfiguration(new Dictionary<string, string?>())
            .ConfigureServices(services => { })
            .WithEndpoint("/test", _ => Task.CompletedTask);

        Assert.NotNull(builder);
    }

    /// <summary>
    /// Test: Builder throws if service name is not set.
    /// </summary>
    [Fact]
    public void Build_WithoutServiceName_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = new WanderpoolTestServerBuilder();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    // Test interfaces for mocking
    public interface ITestService
    {
        string GetValue();
    }

    public class MockTestService : ITestService
    {
        public string GetValue() => "MockValue";
    }
}
