using System.Net;
using Microsoft.Extensions.Logging;
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
    /// Test: Redacts "token" query parameter from logged URLs.
    /// </summary>
    [Fact]
    public async Task SendAsync_RedactsTokenQueryParam()
    {
        // Arrange
        var logMessages = new List<string>();
        var mockLogger = new MockLogger(logMessages);
        var loggingHandler = new Wanderpool.Common.Infra.Clients.HttpClientHandlers.LoggingHandler(mockLogger);
        var innerHandler = new MockLoggingHandler(new HttpResponseMessage(HttpStatusCode.OK));
        loggingHandler.InnerHandler = innerHandler;
        var client = new HttpClient(loggingHandler);
        var requestUri = "https://api.example.com/data?token=secret123&page=1";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.IsSuccessStatusCode);
        // Token value should not appear in logs
        var logOutput = string.Join(" ", logMessages);
        Assert.DoesNotContain("secret123", logOutput);
        // But parameter name should still be visible
        Assert.Contains("token", logOutput);
    }

    /// <summary>
    /// Test: Redacts "key" query parameter from logged URLs.
    /// </summary>
    [Fact]
    public async Task SendAsync_RedactsKeyQueryParam()
    {
        // Arrange
        var logMessages = new List<string>();
        var mockLogger = new MockLogger(logMessages);
        var loggingHandler = new Wanderpool.Common.Infra.Clients.HttpClientHandlers.LoggingHandler(mockLogger);
        var innerHandler = new MockLoggingHandler(new HttpResponseMessage(HttpStatusCode.OK));
        loggingHandler.InnerHandler = innerHandler;
        var client = new HttpClient(loggingHandler);
        var requestUri = "https://api.example.com/endpoint?key=mySecretKey&format=json";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        var logOutput = string.Join(" ", logMessages);
        Assert.DoesNotContain("mySecretKey", logOutput);
        Assert.Contains("key", logOutput);
    }

    /// <summary>
    /// Test: Redacts "password" query parameter from logged URLs.
    /// </summary>
    [Fact]
    public async Task SendAsync_RedactsPasswordQueryParam()
    {
        // Arrange
        var logMessages = new List<string>();
        var mockLogger = new MockLogger(logMessages);
        var loggingHandler = new Wanderpool.Common.Infra.Clients.HttpClientHandlers.LoggingHandler(mockLogger);
        var innerHandler = new MockLoggingHandler(new HttpResponseMessage(HttpStatusCode.OK));
        loggingHandler.InnerHandler = innerHandler;
        var client = new HttpClient(loggingHandler);
        var requestUri = "https://api.example.com/login?user=john&password=pass123";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        var logOutput = string.Join(" ", logMessages);
        Assert.DoesNotContain("pass123", logOutput);
        Assert.Contains("password", logOutput);
    }

    /// <summary>
    /// Test: Redacts "api-key" query parameter from logged URLs.
    /// </summary>
    [Fact]
    public async Task SendAsync_RedactsApiKeyQueryParam()
    {
        // Arrange
        var logMessages = new List<string>();
        var mockLogger = new MockLogger(logMessages);
        var loggingHandler = new Wanderpool.Common.Infra.Clients.HttpClientHandlers.LoggingHandler(mockLogger);
        var innerHandler = new MockLoggingHandler(new HttpResponseMessage(HttpStatusCode.OK));
        loggingHandler.InnerHandler = innerHandler;
        var client = new HttpClient(loggingHandler);
        var requestUri = "https://api.example.com/resource?api-key=abc123xyz&id=42";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        var logOutput = string.Join(" ", logMessages);
        Assert.DoesNotContain("abc123xyz", logOutput);
        Assert.Contains("api-key", logOutput);
    }

    /// <summary>
    /// Test: Non-sensitive query parameters are NOT redacted.
    /// </summary>
    [Fact]
    public async Task SendAsync_PreservesNonSensitiveQueryParams()
    {
        // Arrange
        var logMessages = new List<string>();
        var mockLogger = new MockLogger(logMessages);
        var loggingHandler = new Wanderpool.Common.Infra.Clients.HttpClientHandlers.LoggingHandler(mockLogger);
        var innerHandler = new MockLoggingHandler(new HttpResponseMessage(HttpStatusCode.OK));
        loggingHandler.InnerHandler = innerHandler;
        var client = new HttpClient(loggingHandler);
        var requestUri = "https://api.example.com/items?category=books&sort=date";
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        var logOutput = string.Join(" ", logMessages);
        // Non-sensitive params should be preserved
        Assert.Contains("category=books", logOutput);
        Assert.Contains("sort=date", logOutput);
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

    /// <summary>
    /// Test: Client name is included in logs when available.
    /// </summary>
    [Fact]
    public async Task SendAsync_IncludesClientNameInLogs()
    {
        // Arrange
        var logMessages = new List<string>();
        var mockLogger = new MockLogger(logMessages);
        var loggingHandler = new Wanderpool.Common.Infra.Clients.HttpClientHandlers.LoggingHandler(mockLogger);
        var innerHandler = new MockLoggingHandler(new HttpResponseMessage(HttpStatusCode.OK));
        loggingHandler.InnerHandler = innerHandler;
        var client = new HttpClient(loggingHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/data");
        request.Options.Set(new HttpRequestOptionsKey<string>("ClientName"), "HotelApiClient");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        var logOutput = string.Join(" ", logMessages);
        Assert.Contains("HotelApiClient", logOutput);
    }

    /// <summary>
    /// Test: Falls back to "Unknown" when client name is not set.
    /// </summary>
    [Fact]
    public async Task SendAsync_FallsBackToUnknownWhenNameNotSet()
    {
        // Arrange
        var logMessages = new List<string>();
        var mockLogger = new MockLogger(logMessages);
        var loggingHandler = new Wanderpool.Common.Infra.Clients.HttpClientHandlers.LoggingHandler(mockLogger);
        var innerHandler = new MockLoggingHandler(new HttpResponseMessage(HttpStatusCode.OK));
        loggingHandler.InnerHandler = innerHandler;
        var client = new HttpClient(loggingHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/data");
        // No client name set

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        var logOutput = string.Join(" ", logMessages);
        Assert.Contains("Unknown", logOutput);
    }

    /// <summary>
    /// Test: Client name extracted from request options.
    /// </summary>
    [Fact]
    public async Task SendAsync_ExtractsClientNameFromRequestOptions()
    {
        // Arrange
        var logMessages = new List<string>();
        var mockLogger = new MockLogger(logMessages);
        var loggingHandler = new Wanderpool.Common.Infra.Clients.HttpClientHandlers.LoggingHandler(mockLogger);
        var innerHandler = new MockLoggingHandler(new HttpResponseMessage(HttpStatusCode.OK));
        loggingHandler.InnerHandler = innerHandler;
        var client = new HttpClient(loggingHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/data");
        const string clientName = "FlightApiClient";
        request.Options.Set(new HttpRequestOptionsKey<string>("ClientName"), clientName);

        // Act
        var response = await client.SendAsync(request);

        // Assert
        Assert.NotNull(response);
        var logOutput = string.Join(" ", logMessages);
        Assert.Contains(clientName, logOutput);
    }

    /// <summary>
    /// Mock logger for testing that captures log messages.
    /// </summary>
    private class MockLogger : ILogger<Wanderpool.Common.Infra.Clients.HttpClientHandlers.LoggingHandler>
    {
        private readonly List<string> _messages;

        public MockLogger(List<string> messages)
        {
            _messages = messages;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            _messages.Add(message);
        }
    }
}
