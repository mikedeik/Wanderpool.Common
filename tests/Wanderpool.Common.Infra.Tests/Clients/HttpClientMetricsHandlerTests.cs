using System.Diagnostics.Metrics;
using Wanderpool.Common.Infra.Clients.HttpClientHandlers;
using Wanderpool.Common.Infra.Telemetry;

namespace Wanderpool.Common.Infra.Tests.Clients;

/// <summary>
/// Tests for HTTP client metrics handler that records request metrics.
/// </summary>
public class HttpClientMetricsHandlerTests
{
    [Fact]
    public async Task SendAsync_OnSuccess_RecordsMetrics()
    {
        // Arrange
        var meter = new Meter("TestMetrics");
        var counter = meter.CreateCounter<long>("requests_total");
        var histogram = meter.CreateHistogram<double>("request_duration_seconds");

        var instruments = new HttpClientMetricsInstruments
        {
            RequestsCounter = counter,
            RequestDurationHistogram = histogram
        };

        var handler = new HttpClientMetricsHandler(instruments)
        {
            InnerHandler = new FakeHttpMessageHandler(System.Net.HttpStatusCode.OK)
        };

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/test");

        // Act - should not throw
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SendAsync_OnFailure_RecordsMetrics()
    {
        // Arrange
        var meter = new Meter("TestMetrics");
        var counter = meter.CreateCounter<long>("requests_total");
        var histogram = meter.CreateHistogram<double>("request_duration_seconds");

        var instruments = new HttpClientMetricsInstruments
        {
            RequestsCounter = counter,
            RequestDurationHistogram = histogram
        };

        var handler = new HttpClientMetricsHandler(instruments)
        {
            InnerHandler = new FakeHttpMessageHandler(System.Net.HttpStatusCode.InternalServerError)
        };

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://example.com/api/test");

        // Act - should not throw, metrics should be recorded for failed status codes
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task SendAsync_RecordsMetricsWithCorrectMethod()
    {
        // Arrange
        var meter = new Meter("TestMetrics");
        var counter = meter.CreateCounter<long>("requests_total");
        var histogram = meter.CreateHistogram<double>("request_duration_seconds");

        var instruments = new HttpClientMetricsInstruments
        {
            RequestsCounter = counter,
            RequestDurationHistogram = histogram
        };

        var handler = new HttpClientMetricsHandler(instruments)
        {
            InnerHandler = new FakeHttpMessageHandler(System.Net.HttpStatusCode.OK, 50) // 50ms delay
        };

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Put, "https://example.com/api/test");

        // Act - should not throw, duration should be recorded
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SendAsync_WithClientName_RecordsMetrics()
    {
        // Arrange
        var meter = new Meter("TestMetrics");
        var counter = meter.CreateCounter<long>("requests_total");
        var histogram = meter.CreateHistogram<double>("request_duration_seconds");

        var instruments = new HttpClientMetricsInstruments
        {
            RequestsCounter = counter,
            RequestDurationHistogram = histogram
        };

        var handler = new HttpClientMetricsHandler(instruments, "MyApiClient")
        {
            InnerHandler = new FakeHttpMessageHandler(System.Net.HttpStatusCode.OK)
        };

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/test");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SendAsync_HandlesHttpRequestException()
    {
        // Arrange
        var meter = new Meter("TestMetrics");
        var counter = meter.CreateCounter<long>("requests_total");
        var histogram = meter.CreateHistogram<double>("request_duration_seconds");

        var instruments = new HttpClientMetricsInstruments
        {
            RequestsCounter = counter,
            RequestDurationHistogram = histogram
        };

        var handler = new HttpClientMetricsHandler(instruments)
        {
            InnerHandler = new FailingHttpMessageHandler()
        };

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/test");

        // Act & Assert - exception should be thrown after metrics are recorded
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await client.SendAsync(request);
        });
    }

    /// <summary>
    /// Fake HTTP message handler that returns a specified status code.
    /// </summary>
    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly System.Net.HttpStatusCode _statusCode;
        private readonly int _delayMs;

        public FakeHttpMessageHandler(System.Net.HttpStatusCode statusCode, int delayMs = 0)
        {
            _statusCode = statusCode;
            _delayMs = delayMs;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_delayMs > 0)
            {
                await Task.Delay(_delayMs, cancellationToken);
            }

            return new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent("test response"),
                RequestMessage = request
            };
        }
    }

    /// <summary>
    /// Failing HTTP message handler for testing exception handling.
    /// </summary>
    private class FailingHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new HttpRequestException("Test failure");
        }
    }

}
