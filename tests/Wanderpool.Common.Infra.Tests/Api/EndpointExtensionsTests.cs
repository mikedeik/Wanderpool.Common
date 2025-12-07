using Wanderpool.Common.Infra.Api;
using Xunit;

namespace Wanderpool.Common.Infra.Tests.Api;

/// <summary>
/// Tests for EndpointExtensions - extension methods for endpoint grouping and configuration.
/// </summary>
public class EndpointExtensionsTests
{
    /// <summary>
    /// Test: MapApiGroup extension method exists and is accessible.
    /// </summary>
    [Fact]
    public void MapApiGroup_ExtensionMethodExists()
    {
        // Assert - the extension method should exist and compile
        // If this test compiles, the extension method is accessible
        var mapApiGroupMethods = typeof(EndpointExtensions).GetMethods()
            .Where(m => m.Name == "MapApiGroup")
            .ToList();

        Assert.NotEmpty(mapApiGroupMethods);
    }

    /// <summary>
    /// Test: MapApiGroup has overload without prefix parameter.
    /// </summary>
    [Fact]
    public void MapApiGroup_HasOverload_WithoutPrefix()
    {
        // Assert - should have an overload that takes only RouteGroupBuilder
        var mapApiGroupMethods = typeof(EndpointExtensions).GetMethods()
            .Where(m => m.Name == "MapApiGroup" && m.GetParameters().Length == 1)
            .ToList();

        Assert.NotEmpty(mapApiGroupMethods);
    }

    /// <summary>
    /// Test: MapApiGroup has overload with prefix parameter.
    /// </summary>
    [Fact]
    public void MapApiGroup_HasOverload_WithPrefix()
    {
        // Assert - should have an overload that takes RouteGroupBuilder and string prefix
        var mapApiGroupMethods = typeof(EndpointExtensions).GetMethods()
            .Where(m => m.Name == "MapApiGroup" && m.GetParameters().Length == 2)
            .ToList();

        Assert.NotEmpty(mapApiGroupMethods);
    }

    /// <summary>
    /// Test: MapVersionedApi extension method exists and is accessible.
    /// </summary>
    [Fact]
    public void MapVersionedApi_ExtensionMethodExists()
    {
        // Assert - the extension method should exist and compile
        var mapVersionedApiMethods = typeof(EndpointExtensions).GetMethods()
            .Where(m => m.Name == "MapVersionedApi")
            .ToList();

        Assert.NotEmpty(mapVersionedApiMethods);
    }

    /// <summary>
    /// Test: MapVersionedApi has version parameter.
    /// </summary>
    [Fact]
    public void MapVersionedApi_HasVersionParameter()
    {
        // Assert - the method should accept RouteGroupBuilder and int version
        var mapVersionedApiMethods = typeof(EndpointExtensions).GetMethods()
            .Where(m => m.Name == "MapVersionedApi" && m.GetParameters().Length == 2)
            .ToList();

        Assert.NotEmpty(mapVersionedApiMethods);
    }
}
