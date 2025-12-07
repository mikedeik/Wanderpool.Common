using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using Wanderpool.Common.Infra.Telemetry;

namespace Wanderpool.Common.Infra.Tests.Telemetry;

/// <summary>
/// Tests for custom HTTP client metrics instrumentation.
/// </summary>
public class HttpClientMetricsTests
{
    [Fact]
    public void AddWanderpoolHttpClientMetrics_CreatesMetricInstruments()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddWanderpoolMetrics();
        services.AddWanderpoolHttpClientMetrics();

        // Assert
        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider);
    }

    [Fact]
    public void AddWanderpoolHttpClientMetrics_CounterIncrementable()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddWanderpoolMetrics();

        // Act
        services.AddWanderpoolHttpClientMetrics();
        var provider = services.BuildServiceProvider();
        var meterProvider = provider.GetRequiredService<MeterProvider>();

        // Assert
        Assert.NotNull(meterProvider);
        Assert.NotNull(provider.GetRequiredService<HttpClientMetricsInstruments>());
    }

    [Fact]
    public void AddWanderpoolHttpClientMetrics_RegistersHttpClientMetricsInstruments()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddWanderpoolMetrics();

        // Act
        services.AddWanderpoolHttpClientMetrics();

        // Assert
        var provider = services.BuildServiceProvider();
        var instruments = provider.GetRequiredService<HttpClientMetricsInstruments>();
        Assert.NotNull(instruments);
        Assert.NotNull(instruments.RequestsCounter);
        Assert.NotNull(instruments.RequestDurationHistogram);
    }

    [Fact]
    public void AddWanderpoolHttpClientMetrics_ReturnsServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddWanderpoolMetrics();

        // Act
        var result = services.AddWanderpoolHttpClientMetrics();

        // Assert
        Assert.Equal(services, result);
    }

    [Fact]
    public void HttpClientMetricsInstruments_ContainsCounterAndHistogram()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddWanderpoolMetrics();
        services.AddWanderpoolHttpClientMetrics();

        // Act
        var provider = services.BuildServiceProvider();
        var instruments = provider.GetRequiredService<HttpClientMetricsInstruments>();

        // Assert
        Assert.NotNull(instruments.RequestsCounter);
        Assert.NotNull(instruments.RequestDurationHistogram);
    }
}
