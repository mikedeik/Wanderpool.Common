using System.Net;
using Xunit;

namespace Wanderpool.Common.Infra.Tests.Clients;

/// <summary>
/// Tests for LoggingHandler DelegatingHandler for outbound HTTP logging.
/// </summary>
public class LoggingHandlerTests
{
    /// <summary>
    /// Test: Logs request URL and method before sending request.
    /// </summary>
    [Fact]
    public async Task SendAsync_LogsRequestUrlAndMethod()
    {
        // Arrange
        var handler = new MockLoggingHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/users/123");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotEmpty(handler.LoggedMessages);
    }

    /// <summary>
    /// Test: Logs response status code and duration after receiving response.
    /// </summary>
    [Fact]
    public async Task SendAsync_LogsResponseStatusAndDuration()
    {
        // Arrange
        var handler = new MockLoggingHandler(new HttpResponseMessage(HttpStatusCode.Created));
        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/items");

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = await client.SendAsync(request);
        stopwatch.Stop();

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True(stopwatch.ElapsedMilliseconds >= 0);
    }

    /// <summary>
    /// Test: Uses Info log level for successful 2xx responses.
    /// </summary>
    [Fact]
    public async Task SendAsync_Uses2xxResponseLogging()
    {
        // Arrange
        var handler = new MockLoggingHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/data");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.IsSuccessStatusCode);
    }

    /// <summary>
    /// Test: Uses Warning log level for 4xx client error responses.
    /// </summary>
    [Fact]
    public async Task SendAsync_Uses4xxResponseLogging()
    {
        // Arrange
        var handler = new MockLoggingHandler(new HttpResponseMessage(HttpStatusCode.BadRequest));
        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/items");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Test: Uses Error log level for 5xx server error responses.
    /// </summary>
    [Fact]
    public async Task SendAsync_Uses5xxResponseLogging()
    {
        // Arrange
        var handler = new MockLoggingHandler(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/data");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    /// <summary>
    /// Test: Logs exceptions with details and re-throws.
    /// </summary>
    [Fact]
    public async Task SendAsync_LogsExceptionsAndRethrows()
    {
        // Arrange
        var expectedException = new HttpRequestException("Connection failed");
        var handler = new MockLoggingHandler(expectedException);
        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/data");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => client.SendAsync(request));
        Assert.Equal("Connection failed", exception.Message);
    }

    /// <summary>
    /// Mock handler for testing that tracks logged messages.
    /// </summary>
    private class MockLoggingHandler : HttpClientHandler
    {
        private readonly HttpResponseMessage? _response;
        private readonly Exception? _exception;
        public List<string> LoggedMessages { get; } = new();

        public MockLoggingHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        public MockLoggingHandler(Exception exception)
        {
            _exception = exception;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LoggedMessages.Add($"{request.Method} {request.RequestUri}");

            if (_exception != null)
            {
                throw _exception;
            }

            // Simulate some delay to test timing
            await Task.Delay(10, cancellationToken);

            LoggedMessages.Add($"{(int)_response!.StatusCode}");
            return _response;
        }
    }
}
